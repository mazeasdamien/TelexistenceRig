using FRRobot;
using Microsoft.Azure.Kinect.Sensor;
using Rti.Dds.Core;
using Rti.Dds.Domain;
using Rti.Dds.Publication;
using Rti.Dds.Subscription;
using Rti.Dds.Topics;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class TelexistenceRigProgram
    {
        public static bool consoleOpen = true;
        public static DomainParticipant domainParticipant = null!;
        public static QosProvider provider = null!;
        public static Publisher publisher = null!;
        public static Subscriber subscriber = null!;
        public static int sampleId = 1;
        public static bool isSimulation;
        public static bool useVideo;
        public static bool usePointcloud;

        //Fanuc
        public static readonly FRCRobot fRCRobot = new();
        public static FRCSysInfo fRCSysInfo = null!;
        public static bool isReachable;
        public static bool isTeleop;

        //Kinects
        public static Device kinect0 = null!;
        public static Device kinect1 = null!;

        //Threads
        public static Thread SUB_OperatorRequests = null!;
        public static Thread PUB_RobotState = null!;
        public static Thread PUB_RobotAlarm = null!;
        public static Thread SUB_Teleop = null!;
        public static Thread PUB_Reachability = null!;
        public static Thread SUB_Path = null!;
        public static Thread PUB_RobotImage = null!;
        public static Thread PUB_RobotPointCloud = null!;

        public static void Main()
        {
            //catch CTRL-C event to exit console cleanly and stop kinects
             Console.CancelKeyPress += delegate { Shutdown(); };

            #region User choices
            //config
            bool showMenu1 = true;
            while (showMenu1)
            {
                showMenu1 = Menu1();
            }

            bool showMenu2 = true;
            while (showMenu2)
            {
                showMenu2 = Menu2();
            }

            bool showMenu3 = true;
            while (showMenu3)
            {
                showMenu3 = Menu3();
            }

            Console.Clear();
            #endregion

            //Initialize Robot
            Console.WriteLine("Waiting for Robot ...");
            if (isSimulation)
            {
                //local Roboguide IP address
                fRCRobot.ConnectEx("127.0.0.1", false, 10, 1);
            }
            else
            {
                //Cranfield University local IP address (Laptop needs to be setup on 192.168.1.19)
                fRCRobot.ConnectEx("192.168.1.20", false, 10, 1);
            }
            isReachable = false;
            isTeleop = false;

            //Run Publishers and Subscribers if connected to Fanuc Robot
            if (fRCRobot.IsConnected)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Robot Connected");

                //Initialize DDS
                Console.WriteLine("Starting DDS on domain 0 with config TelexistenceRig.xml");
                provider = new QosProvider("TelexistenceRig.xml");
                domainParticipant = DomainParticipantFactory.Instance.CreateParticipant(0, provider.GetDomainParticipantQos());
                var publisherQos = provider.GetPublisherQos("RigQoSLibrary::RigQoSProfile");
                publisher = domainParticipant.CreatePublisher(publisherQos);
                var subscriberQos = provider.GetSubscriberQos("RigQoSLibrary::RigQoSProfile");
                subscriber = domainParticipant.CreateSubscriber(subscriberQos);

                fRCSysInfo = fRCRobot.SysInfo;

                #region config kinects
                if (useVideo && !usePointcloud)
                {
                    kinect0 = Device.Open();
                    kinect0.StartCameras(new DeviceConfiguration
                    {
                        ColorFormat = ImageFormat.ColorMJPG,
                        ColorResolution = ColorResolution.R720p,
                        DepthMode = DepthMode.NFOV_2x2Binned,
                        SynchronizedImagesOnly = true,
                        CameraFPS = FPS.FPS30
                    });
                    PUB_RobotImage = new(() => RobotImagePublisher.RunPublisher());
                    PUB_RobotImage.Start();
                }
                else if (useVideo && usePointcloud)
                {
                    kinect0 = Device.Open(0);
                    kinect1 = Device.Open(1);
                    kinect0.StartCameras(new DeviceConfiguration
                    {
                        ColorFormat = ImageFormat.ColorMJPG,
                        ColorResolution = ColorResolution.R720p,
                        DepthMode = DepthMode.NFOV_2x2Binned,
                        SynchronizedImagesOnly = true,
                        CameraFPS = FPS.FPS30
                    });
                    kinect1.StartCameras(new DeviceConfiguration
                    {
                        ColorFormat = ImageFormat.ColorBGRA32,
                        ColorResolution = ColorResolution.R720p,
                        DepthMode = DepthMode.NFOV_2x2Binned,
                        SynchronizedImagesOnly = true,
                        CameraFPS = FPS.FPS30
                    });
                    PUB_RobotImage = new(() => RobotImagePublisher.RunPublisher());
                    PUB_RobotPointCloud = new(() => RobotPointCloudPublisher.RunPublisher());
                    PUB_RobotImage.Start();
                    PUB_RobotPointCloud.Start();
                }
                else if (!useVideo && usePointcloud)
                {
                    kinect1 = Device.Open();
                    kinect1.StartCameras(new DeviceConfiguration
                    {
                        ColorFormat = ImageFormat.ColorBGRA32,
                        ColorResolution = ColorResolution.R720p,
                        DepthMode = DepthMode.NFOV_2x2Binned,
                        SynchronizedImagesOnly = true,
                        CameraFPS = FPS.FPS30
                    });
                    PUB_RobotPointCloud = new(() => RobotPointCloudPublisher.RunPublisher());
                    PUB_RobotPointCloud.Start();
                }
                #endregion

                SUB_OperatorRequests = new(() => OperatorRequestsSubscriber.RunSubscriber());
                PUB_RobotState = new(() => RobotStatePublisher.RunPublisher());
                PUB_RobotAlarm = new(() => RobotAlarmPublisher.RunPublisher());
                SUB_Teleop = new(() => TeleopSubscriber.RunSubscriber());
                PUB_Reachability = new(() => ReachabilityPublisher.RunPublisher());
                SUB_Path = new(() => PathSubscriber.RunSubscriber());

                SUB_OperatorRequests.Start();
                PUB_RobotState.Start();
                PUB_RobotAlarm.Start();
                SUB_Teleop.Start();
                PUB_Reachability.Start();
                SUB_Path.Start();
            }
        }

        private static void Shutdown()
        {
            Console.WriteLine("Clean exit in progress ...");
            SUB_OperatorRequests.Interrupt();
            PUB_RobotState.Interrupt();
            PUB_RobotAlarm.Interrupt();
            SUB_Teleop.Interrupt();
            PUB_Reachability.Interrupt();
            SUB_Path.Interrupt();
            if (useVideo)
            {
                PUB_RobotImage.Interrupt();
                kinect0.StopCameras();
            }
            if (usePointcloud)
            {
                PUB_RobotPointCloud.Interrupt();
                kinect1.StopCameras();
            }
            domainParticipant.Dispose();
            Environment.Exit(0);
        }

        public static DataWriter<DynamicData> SetupDataWriter(string topicName, Publisher publisher, DynamicType dynamicData)
        {
            DataWriter<DynamicData> writer;
            Topic<DynamicData> topic = domainParticipant.CreateTopic(topicName, dynamicData);
            var writerQos = provider.GetDataWriterQos("RigQoSLibrary::RigQoSProfile");
            writer = publisher.CreateDataWriter(topic, writerQos);
            return writer;
        }

        public static DataReader<DynamicData> SetupDataReader(string topicName, Subscriber subscriber, DynamicType dynamicData)
        {
            DataReader<DynamicData> reader;
            Topic<DynamicData> topic = domainParticipant.CreateTopic(topicName, dynamicData);
            var readerQos = provider.GetDataReaderQos("RigQoSLibrary::RigQoSProfile");
            reader = subscriber.CreateDataReader(topic, readerQos);
            return reader;
        }

        #region config menu
        private static bool Menu1()
        {
            Console.Clear();
            Console.WriteLine("1) Simulation Roboguide");
            Console.WriteLine("2) Physical Robot");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    isSimulation = true;
                    return false;
                case "2":
                    isSimulation = false;
                    return false;
                default:
                    return true;
            }
        }
        private static bool Menu2()
        {
            Console.Clear();
            Console.WriteLine("Use Kinect video:");
            Console.WriteLine("1) yes");
            Console.WriteLine("2) no");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    useVideo = true;
                    return false;
                case "2":
                    useVideo = false;
                    return false;
                default:
                    return true;
            }
        }
        private static bool Menu3()
        {
            Console.Clear();
            Console.WriteLine("Use Kinect pointcloud:");
            Console.WriteLine("1) yes");
            Console.WriteLine("2) no");
            Console.Write("\r\nSelect an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    usePointcloud = true;
                    return false;
                case "2":
                    usePointcloud = false;
                    return false;
                default:
                    return true;
            }
        }
        #endregion
    }
}