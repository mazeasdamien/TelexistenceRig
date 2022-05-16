using Rti.Dds.Publication;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class ReachabilityPublisher : TelexistenceRigProgram
    {
        public static void RunPublisher()
        {
            int counter = 1;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");

            var typeFactory = DynamicTypeFactory.Instance;
            StructType Reachable = typeFactory.BuildStruct()
                .WithName("Reachability")
                .AddMember(new StructMember("Clock", typeFactory.CreateString(bounds: 50)))
                .AddMember(new StructMember("Sample", typeFactory.GetPrimitiveType<int>()))
                .AddMember(new StructMember("isReachable", typeFactory.GetPrimitiveType<bool>()))
                .Create();

            DataWriter<DynamicData> writer = SetupDataWriter("Reachability_Topic", publisher, Reachable);
            var sample = new DynamicData(Reachable);

            while (consoleOpen)
            {
                if (isTeleop)
                {
                    sample.SetValue(memberName: "Clock", fRCSysInfo.Clock.ToString());
                    sample.SetValue(memberName: "Sample", sampleId);
                    sample.SetValue(memberName: "isReachable", isReachable);

                    if (counter % 5000 == 0 && counter != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.Write("SAMPLE isReachable: " + counter);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(" TOTAL: " + sampleId);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" " + fRCSysInfo.Clock.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($" Operator new position reachablility checking");
                    }
                    sampleId++;
                    counter++;
                    writer.Write(sample);
                    Thread.Sleep(100);
                }
            }
        }
    }
}
