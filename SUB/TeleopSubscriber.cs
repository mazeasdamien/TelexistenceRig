using FRRobot;
using Rti.Dds.Subscription;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class TeleopSubscriber : TelexistenceRigProgram
    {
        public static void RunSubscriber()
        {
            var typeFactory = DynamicTypeFactory.Instance;

            StructType OperatorNewPose = typeFactory.BuildStruct()
               .WithName("OperatorNewPose")
               .AddMember(new StructMember("X", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("Y", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("Z", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("W", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("P", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("R", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("Samples", typeFactory.GetPrimitiveType<int>()))
               .Create();

            DataReader<DynamicData> reader = SetupDataReader("OperatorNewPose_Topic", subscriber, OperatorNewPose);
            FRCSysPositions fRCTPPositions = fRCRobot.RegPositions;

            while (consoleOpen)
            {
                using var samples = reader.Take();
                foreach (var sample in samples)
                {
                    if (sample.Info.ValidData)
                    {
                        DynamicData data = sample.Data;
                        FRCSysPosition sysPosition = fRCTPPositions[3];
                        FRCSysGroupPosition sysGroupPosition = sysPosition.Group[1];
                        FRCXyzWpr xyzWpr = sysGroupPosition.Formats[FRETypeCodeConstants.frXyzWpr];
                        xyzWpr.X = data.GetValue<float>("X");
                        xyzWpr.Y = data.GetValue<float>("Y");
                        xyzWpr.Z = data.GetValue<float>("Z");
                        xyzWpr.W = data.GetValue<float>("W");
                        xyzWpr.P = data.GetValue<float>("P");
                        xyzWpr.R = data.GetValue<float>("R");

                        if (sysGroupPosition.IsReachable[Type.Missing, FREMotionTypeConstants.frJointMotionType, FREOrientTypeConstants.frAESWorldOrientType, Type.Missing, out _])
                        {
                            isReachable = true;
                            sysGroupPosition.Update();
                        }
                        else
                        {
                            isReachable = false;
                        }
                    }
                }
            }
        }
    }
}