using Rti.Dds.Subscription;
using Rti.Types.Dynamic;

namespace TelexistenceRig
{
    public class PathSubscriber : TelexistenceRigProgram
    {
        public static void RunSubscriber()
        {
            var typeFactory = DynamicTypeFactory.Instance;

            StructType Path_position = typeFactory.BuildStruct()
               .WithName("Path_position")
               .AddMember(new StructMember("ID", typeFactory.GetPrimitiveType<int>()))
               .AddMember(new StructMember("isUpdating", typeFactory.GetPrimitiveType<bool>()))
               .AddMember(new StructMember("isDelete", typeFactory.GetPrimitiveType<bool>()))
               .AddMember(new StructMember("X", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("Y", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("Z", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("W", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("P", typeFactory.GetPrimitiveType<float>()))
               .AddMember(new StructMember("R", typeFactory.GetPrimitiveType<float>()))
               .Create();

            DataReader<DynamicData> reader = SetupDataReader("Path_position_Topic", subscriber, Path_position);

            while (consoleOpen)
            {
                using var samples = reader.Take();
                foreach (var sample in samples)
                {
                    if (sample.Info.ValidData)
                    {
                        DynamicData data = sample.Data;
                    }
                }
            }
        }
    }
}
