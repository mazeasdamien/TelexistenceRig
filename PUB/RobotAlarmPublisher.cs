using FRRobot;
using Rti.Dds.Publication;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class RobotAlarmPublisher : TelexistenceRigProgram
    {
        public static void RunPublisher()
        {
            int counter = 1;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");

            var typeFactory = DynamicTypeFactory.Instance;
            StructType RobotAlarm = typeFactory.BuildStruct()
                .WithName("RobotAlarm")
                .AddMember(new StructMember("Clock", typeFactory.CreateString(bounds: 50)))
                .AddMember(new StructMember("Sample", typeFactory.GetPrimitiveType<int>()))
                .AddMember(new StructMember("Alarm", typeFactory.CreateString(bounds: 200)))
                .Create();

            DataWriter<DynamicData> writer = SetupDataWriter("RobotAlarm_Topic", publisher, RobotAlarm);
            var sample = new DynamicData(RobotAlarm);

            string tempAlarm = "";

            while (consoleOpen)
            {
                FRCAlarms fRCAlarms = fRCRobot.Alarms;

                if (tempAlarm != fRCAlarms[0].ErrorMessage)
                {
                    sample.SetValue(memberName: "Clock", fRCSysInfo.Clock.ToString());
                    sample.SetValue(memberName: "Sample", sampleId);
                    sample.SetValue(memberName: "Alarm", fRCAlarms[0].ErrorMessage);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("SAMPLE ALARM: " + counter);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(" TOTAL: " + sampleId);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" " + fRCSysInfo.Clock.ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(" " + fRCAlarms[0].ErrorMessage);

                    sampleId++;
                    counter++;
                    tempAlarm = fRCAlarms[0].ErrorMessage;
                    writer.Write(sample);
                }
                Thread.Sleep(10);
            }
        }
    }
}
