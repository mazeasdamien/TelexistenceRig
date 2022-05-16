using Microsoft.Azure.Kinect.Sensor;
using Rti.Dds.Publication;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class RobotPointCloudPublisher : TelexistenceRigProgram
    {
        public async static void RunPublisher()
        {
            int counter = 1;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
            Transformation transformation;
            transformation = kinect1.GetCalibration().CreateTransformation();

            var typeFactory = DynamicTypeFactory.Instance;
            StructType RobotPointCloud = typeFactory.BuildStruct()
                .WithName("RobotPointCloud")
                .AddMember(new StructMember("Clock", typeFactory.CreateString(bounds: 50)))
                .AddMember(new StructMember("Sample", typeFactory.GetPrimitiveType<int>()))
                .AddMember(new StructMember("sequence_Memory", typeFactory.CreateSequence(typeFactory.GetPrimitiveType<float>(), 600000)))
                .Create();

            DataWriter<DynamicData> writer = SetupDataWriter("RobotPointCloud_Topic", publisher, RobotPointCloud);
            var sample = new DynamicData(RobotPointCloud);

            List<float> pointcloudData = new();
            while (consoleOpen)
            {
                pointcloudData.Clear();
                Image modifiedColor;
                Image cloudImage;

                using (Capture capture = await Task.Run(() => { return kinect1.GetCapture(); }))
                {
                    modifiedColor = transformation.ColorImageToDepthCamera(capture);
                    cloudImage = transformation.DepthImageToPointCloud(capture.Depth);
                }

                BGRA[] colorArray = modifiedColor.GetPixels<BGRA>().ToArray();
                Short3[] PointCloud = cloudImage.GetPixels<Short3>().ToArray();

                for (int i = 0; i < PointCloud.Length; i++)
                {
                    if (colorArray[i].B != 0 || colorArray[i].G != 0 || colorArray[i].R != 0)
                    {
                        if (PointCloud[i].Z <= 1500)
                        {
                            pointcloudData.Add(PointCloud[i].X);
                            pointcloudData.Add(PointCloud[i].Y);
                            pointcloudData.Add(PointCloud[i].Z);

                            pointcloudData.Add(colorArray[i].B);
                            pointcloudData.Add(colorArray[i].G);
                            pointcloudData.Add(colorArray[i].R);
                        }
                    }
                }

                sample.SetValue("Clock", fRCSysInfo.Clock.ToString());
                sample.SetValue("Sample", sampleId);
                sample.SetValue("sequence_Memory", pointcloudData.ToArray());

                if (counter % 500 == 0 && counter != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write("SAMPLE POINTCLOUD: " + counter);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(" TOTAL: " + sampleId);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" " + fRCSysInfo.Clock.ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" Kinect PointCloud data updated");
                }

                sampleId++;
                counter++;
                writer.Write(sample);
                Thread.Sleep(38);
            }
        }
    }
}