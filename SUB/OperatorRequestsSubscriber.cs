using FRRobot;
using Rti.Dds.Subscription;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class OperatorRequestsSubscriber : TelexistenceRigProgram
    {
        public static void RunSubscriber()
        {
            var typeFactory = DynamicTypeFactory.Instance;
            var OperatorButtons = typeFactory.BuildEnum()
                .WithName("OperatorButtons")
                .AddMember(new EnumMember("RESET", ordinal: 0))
                .AddMember(new EnumMember("ABORT", ordinal: 1))
                .AddMember(new EnumMember("HOME", ordinal: 2))
                .AddMember(new EnumMember("PATH", ordinal: 3))
                .Create();

            var OperatorRequest = typeFactory.BuildStruct()
               .WithName("OR")
               .AddMember(new StructMember("Buttons", OperatorButtons))
               .AddMember(new StructMember("S", typeFactory.GetPrimitiveType<int>()))
               .Create();

            DataReader<DynamicData> reader = SetupDataReader("OR_Topic", subscriber, OperatorRequest);
            FRCAlarms fRCAlarms = fRCRobot.Alarms;
            FRCTasks mobjTasks = fRCRobot.Tasks;
            FRCPrograms fRCPrograms = fRCRobot.Programs;

            while (consoleOpen)
            {
                using var samples = reader.Take();
                foreach (var sample in samples)
                {
                    if (sample.Info.ValidData)
                    {
                        DynamicData data = sample.Data;



                        if (data.GetValue<ulong>("Buttons") == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"OPERATOR REQUEST {data.GetValue<int>("S")} = RESET CONTROLLER");
                            fRCAlarms.Reset();
                        }
                        else if (data.GetValue<ulong>("Buttons") == 1)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"OPERATOR REQUEST {data.GetValue<int>("S")} = ABORT ALL");
                            mobjTasks.AbortAll();
                        }
                        else if (data.GetValue<ulong>("Buttons") == 2)
                        {
                            isTeleop = false;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"OPERATOR REQUEST {data.GetValue<int>("S")} = HOME PROGRAM");
                            Thread.Sleep(500);
                            mobjTasks.AbortAll();
                            Thread.Sleep(500);
                            fRCAlarms.Reset();
                            fRCPrograms.Selected = "ZERO";
                            FRCTPProgram fRCProgram = (FRCTPProgram)fRCPrograms[fRCPrograms.Selected, Type.Missing, Type.Missing];
                            fRCProgram.Run();
                        }
                        else if (data.GetValue<ulong>("Buttons") == 3)
                        {
                            isTeleop = true;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"OPERATOR REQUEST {data.GetValue<int>("S")} = TELEOP PROGRAM");
                            Thread.Sleep(500);
                            mobjTasks.AbortAll();
                            Thread.Sleep(500);
                            fRCAlarms.Reset();
                            fRCPrograms.Selected = "DAMIEN_UNITY";
                            FRCTPProgram fRCProgram = (FRCTPProgram)fRCPrograms[fRCPrograms.Selected, Type.Missing, Type.Missing];
                            fRCProgram.Run();
                        }
                    }
                }
            }
        }
    }
}
