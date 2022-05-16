using FRRobot;
using Rti.Dds.Core;
using Rti.Dds.Publication;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class RobotStatePublisher : TelexistenceRigProgram
    {
        public static void RunPublisher()
        {
            int counter = 1;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
            FRCCurPosition curPosition = fRCRobot.CurPosition;
            FRCCurGroupPosition groupPositionJoint = curPosition.Group[1, FRECurPositionConstants.frJointDisplayType];
            FRCCurGroupPosition groupPositionWorld = curPosition.Group[1, FRECurPositionConstants.frWorldDisplayType];

            var typeFactory = DynamicTypeFactory.Instance;
            StructType RobotState = typeFactory.BuildStruct()
                .WithName("RobotState")
                .AddMember(new StructMember("Clock", typeFactory.CreateString(bounds: 50)))
                .AddMember(new StructMember("Sample", typeFactory.GetPrimitiveType<int>()))
                .AddMember(new StructMember("J1", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("J2", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("J3", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("J4", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("J5", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("J6", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("X", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("Y", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("Z", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("W", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("P", typeFactory.GetPrimitiveType<double>()))
                .AddMember(new StructMember("R", typeFactory.GetPrimitiveType<double>()))
                .Create();

            DataWriter<DynamicData> writer = SetupDataWriter("RobotState_Topic", publisher, RobotState);
            var sample = new DynamicData(RobotState);

            List<float> tempJoints = new();
            tempJoints.Add(1);
            tempJoints.Add(1);
            tempJoints.Add(1);
            tempJoints.Add(1);
            tempJoints.Add(1);
            tempJoints.Add(1);

            while (consoleOpen)
            {
                groupPositionJoint.Refresh();
                FRCJoint joint = groupPositionJoint.Formats[FRETypeCodeConstants.frJoint];
                FRCXyzWpr xyzWpr = groupPositionWorld.Formats[FRETypeCodeConstants.frXyzWpr];
                FRCAlarms fRCAlarms = fRCRobot.Alarms;

                sample.SetValue(memberName: "Clock", fRCSysInfo.Clock.ToString());
                sample.SetValue(memberName: "Sample", sampleId);
                sample.SetValue(memberName: "J1", joint[1]);
                sample.SetValue(memberName: "J2", joint[2]);
                sample.SetValue(memberName: "J3", joint[3]);
                sample.SetValue(memberName: "J4", joint[4]);
                sample.SetValue(memberName: "J5", joint[5]);
                sample.SetValue(memberName: "J6", joint[6]);
                sample.SetValue(memberName: "X", xyzWpr.X);
                sample.SetValue(memberName: "Y", xyzWpr.Y);
                sample.SetValue(memberName: "Z", xyzWpr.Z);
                sample.SetValue(memberName: "W", xyzWpr.W);
                sample.SetValue(memberName: "P", xyzWpr.P);
                sample.SetValue(memberName: "R", xyzWpr.R);

                if (tempJoints[0] != (float)joint[1] ||
                    tempJoints[1] != (float)joint[2] ||
                    tempJoints[2] != (float)joint[3] ||
                    tempJoints[3] != (float)joint[4] ||
                    tempJoints[4] != (float)joint[5] ||
                    tempJoints[5] != (float)joint[6])
                {
                    if (counter % 500 == 0 && counter != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("SAMPLE ROBOTSTATE: " + counter);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(" TOTAL: " + sampleId);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" " + fRCSysInfo.Clock.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine(" Fanuc Robot current positions data updated");
                    }
                    sampleId++;
                    counter++;
                    writer.Write(sample);
                    tempJoints.Clear();
                    tempJoints.Add((float)joint[1]);
                    tempJoints.Add((float)joint[2]);
                    tempJoints.Add((float)joint[3]);
                    tempJoints.Add((float)joint[4]);
                    tempJoints.Add((float)joint[5]);
                    tempJoints.Add((float)joint[6]);
                }

            }
        }
    }
}