using FRRobot;
using Microsoft.Azure.Kinect.Sensor;
using Rti.Dds.Publication;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class RobotImagePublisher : TelexistenceRigProgram
    {
        public async static void RunPublisher()
        {
            int counter = 1;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
            FRCCurPosition curPosition = fRCRobot.CurPosition;

            var typeFactory = DynamicTypeFactory.Instance;
            StructType RobotImage = typeFactory.BuildStruct()
                .WithName("Video")
                .AddMember(new StructMember("Clock", typeFactory.CreateString(bounds: 50)))
                .AddMember(new StructMember("Sample", typeFactory.GetPrimitiveType<int>()))
                .AddMember(new StructMember("Memory", typeFactory.CreateSequence(typeFactory.GetPrimitiveType<byte>(), 1000000)))
                .Create();

            DataWriter<DynamicData> writer = SetupDataWriter("Video_Topic", publisher, RobotImage);
            var sample = new DynamicData(RobotImage);

            while (consoleOpen)
            {
                using (Capture capture = await Task.Run(() => { return kinect0.GetCapture(); }))
                {
                    sample.SetValue(memberName: "Clock", value: fRCSysInfo.Clock.ToString());
                    sample.SetValue(memberName: "Sample", value: sampleId);
                    sample.SetValue(memberName: "Memory", value: capture.Color.Memory.ToArray());
                }

                if (counter % 500 == 0 && counter != 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write("SAMPLE KINECTIMAGE: " + counter);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(" TOTAL: " + sampleId);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" " + fRCSysInfo.Clock.ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" Kinect Images data updated");
                }
                sampleId++;
                counter++;
                writer.Write(sample);
            }
        }
    }
}
