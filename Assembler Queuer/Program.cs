using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public static class Items
        {
            public static readonly MyItemType BulletproofGlass = MyItemType.MakeComponent("BulletproofGlass");
            public static readonly MyItemType Canvas = MyItemType.MakeComponent("Canvas");
            public static readonly MyItemType Computer = MyItemType.MakeComponent("Computer");
            public static readonly MyItemType Construction = MyItemType.MakeComponent("Construction");
            public static readonly MyItemType Detector = MyItemType.MakeComponent("Detector");
            public static readonly MyItemType Display = MyItemType.MakeComponent("Display");
            public static readonly MyItemType Explosives = MyItemType.MakeComponent("Explosives");
            public static readonly MyItemType Girder = MyItemType.MakeComponent("Girder");
            public static readonly MyItemType GravityGenerator = MyItemType.MakeComponent("GravityGenerator");
            public static readonly MyItemType InteriorPlate = MyItemType.MakeComponent("InteriorPlate");
            public static readonly MyItemType LargeTube = MyItemType.MakeComponent("LargeTube");
            public static readonly MyItemType Medical = MyItemType.MakeComponent("Medical");
            public static readonly MyItemType MetalGrid = MyItemType.MakeComponent("MetalGrid");
            public static readonly MyItemType Motor = MyItemType.MakeComponent("Motor");
            public static readonly MyItemType PowerCell = MyItemType.MakeComponent("PowerCell");
            public static readonly MyItemType RadioCommunication = MyItemType.MakeComponent("RadioCommunication");
            public static readonly MyItemType Reactor = MyItemType.MakeComponent("Reactor");
            public static readonly MyItemType SmallTube = MyItemType.MakeComponent("SmallTube");
            public static readonly MyItemType SolarCell = MyItemType.MakeComponent("SolarCell");
            public static readonly MyItemType SteelPlate = MyItemType.MakeComponent("SteelPlate");
            public static readonly MyItemType Superconductor = MyItemType.MakeComponent("Superconductor");
            public static readonly MyItemType Thrust = MyItemType.MakeComponent("Thrust");
        }
        public Program()
        {
            // 60 ticks per second.
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }
        // the amount of work to queue with every request.
        // helps balance the workload between every item.
        private MyFixedPoint workUnit = 200;
        // the total number of work units allowed in the queue
        private int maxWorkUnits = 2;
        // which assemblers to examine.  Queuing is balanced between enabled assemblers, not cooperative, in assembler mode.
        private string assemblerGroupName = "Assemblers";
        // which containers to examine for fullness.
        private string partsContainersGroupName = "Parts Containers";

        private Dictionary<MyItemType, MyFixedPoint> AllComponents = new Dictionary<MyItemType, MyFixedPoint>{
            { Items.BulletproofGlass, 1000},
            { Items.Canvas, 1000},
            { Items.Computer, 5000},
            { Items.Construction, 5000},
            { Items.Detector, 1000},
            { Items.Display, 1000},
            { Items.Explosives, 1000},
            { Items.Girder, 1000},
            { Items.GravityGenerator, 1000},
            { Items.InteriorPlate, 10000},
            { Items.LargeTube, 2000},
            { Items.Medical, 1000},
            { Items.MetalGrid, 1000},
            { Items.Motor, 2000},
            { Items.PowerCell, 2000},
            { Items.RadioCommunication, 1000},
            { Items.Reactor, 1000},
            { Items.SmallTube, 1000},
            { Items.SolarCell, 1000},
            { Items.SteelPlate, 10000},
            { Items.Superconductor, 1000},
            { Items.Thrust, 10000}
        };
        private readonly static Dictionary<MyItemType, MyDefinitionId> AllBlueprints = new Dictionary<MyItemType, MyDefinitionId>
        {
            // no magic here.  look it up in SpaceEngineers\Content\Data\Blueprints.sbc
            // the names don't match so you gotta write it out :-(
            // assemblers have a different definition than the implict conversion from MyItemType
            // https://forum.keenswh.com/threads/how-to-add-an-individual-component-to-the-assembler-queue.7393616/
            { Items.BulletproofGlass, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/BulletproofGlass")},
            { Items.Canvas, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Canvas")},
            { Items.Computer, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ComputerComponent")},
            { Items.Construction, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ConstructionComponent")},
            { Items.Detector, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/DetectorComponent")},
            { Items.Display, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Display")},
            { Items.Explosives, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ExplosivesComponent")},
            { Items.Girder, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/GirderComponent")},
            { Items.GravityGenerator, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent")},
            { Items.InteriorPlate, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/InteriorPlate")},
            { Items.LargeTube, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/LargeTube")},
            { Items.Medical, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MedicalComponent")},
            { Items.MetalGrid, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MetalGrid")},
            { Items.Motor, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MotorComponent")},
            { Items.PowerCell, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/PowerCell")},
            { Items.RadioCommunication, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent")},
            { Items.Reactor, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ReactorComponent")},
            { Items.SmallTube, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SmallTube")},
            { Items.SolarCell, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SolarCell")},
            { Items.SteelPlate, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SteelPlate")},
            { Items.Superconductor, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Superconductor")},
            { Items.Thrust, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ThrustComponent")}
        };

        static MyDefinitionId ToBlueprintDefinition(MyItemType type)
        {
            // there is a definition id as MyItemType.Type, but it turns out assemblers only queue blueprints.
            return AllBlueprints[type];
        }

        Dictionary<MyDefinitionId, MyFixedPoint> SumQueued(IEnumerable<IMyAssembler> assemblers)
        {
            // todo to iterate over all assemblers once
            Dictionary<MyDefinitionId, MyFixedPoint> totals = new Dictionary<MyDefinitionId, MyFixedPoint>();
            foreach (IMyAssembler a in assemblers)
            {
                List<MyProductionItem> items = new List<MyProductionItem>();
                a.GetQueue(items);
                foreach (MyProductionItem i in items)
                {
                    MyFixedPoint total;
                    totals.TryGetValue(i.BlueprintId, out total);
                    totals[i.BlueprintId] = total + i.Amount;
                }

            }
            return totals;
        }
        Dictionary<MyItemType, MyFixedPoint> SumStored(IEnumerable<IMyEntity> containers)
        {
            Dictionary<MyItemType, MyFixedPoint> totals = new Dictionary<MyItemType, MyFixedPoint>();
            foreach (IMyEntity container in containers)
            {
                int numInventories = container.InventoryCount;
                for (int i=0; i<numInventories; i++)
                {
                    IMyInventory inventory = container.GetInventory(i);
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    inventory.GetItems(items);
                    foreach (MyInventoryItem item in items) {
                        MyFixedPoint total;
                        totals.TryGetValue(item.Type, out total);
                        totals[item.Type] = total + item.Amount;
                    }
                }
            }
            return totals;
        }
        public void Main(string argument, UpdateType updateSource)
        {

            // for each item:
            //   find total in inventory
            //   find total queued
            //   find missing based on limits
            // prioritize based on %missing
            // enqueue work unit if total < total desired and < max work units

            // check setup...
            bool exit = false;
            IMyBlockGroup assemblerGroup = GridTerminalSystem.GetBlockGroupWithName(assemblerGroupName);
            if (assemblerGroup == null)
            {
                exit = true;
                Echo($"Assembler Group {assemblerGroupName} not found.");
            }
            IMyBlockGroup containerGroup = GridTerminalSystem.GetBlockGroupWithName(partsContainersGroupName);
            if (containerGroup == null)
            {
                exit = true;
                Echo($"Container Group {partsContainersGroupName} not found.");
            }

            List<IMyAssembler> assemblers = new List<IMyAssembler>();
            List<IMyEntity> containers = new List<IMyEntity>();
            List<IMyAssembler> primaries = new List<IMyAssembler>();

            assemblerGroup.GetBlocksOfType(assemblers);
            assemblerGroup.GetBlocksOfType(primaries, a => !a.CooperativeMode && a.Enabled && a.Mode == MyAssemblerMode.Assembly);

            if(primaries.Count == 0)
            {
                exit = true;
                Echo($"No primary assembler found");
            }
            if (exit)
            {
                return;
            }
            // we at least have an assembler that can queue and a container to count.
            containerGroup.GetBlocksOfType(containers);

            // assemblers are also containers so add them in.  Use a HashSet for deduping
            HashSet<IMyEntity> allContainers = new HashSet<IMyEntity>();
            allContainers.UnionWith(containers);
            allContainers.UnionWith(assemblers);

            // sum it all up!
            Dictionary<MyDefinitionId, MyFixedPoint> allQueued = SumQueued(assemblers);
            Dictionary<MyItemType, MyFixedPoint> allStored = SumStored(allContainers);

            // used for round robin balancing of work
            int queueStep = 0;
            // queue up work for every component we want built.
            foreach (KeyValuePair<MyItemType, MyFixedPoint> kv in AllComponents)
            {
                Echo($"Checking queue for {kv.Key.ToString()}");
                MyFixedPoint queued;
                MyFixedPoint stored;
                allQueued.TryGetValue(ToBlueprintDefinition(kv.Key), out queued);
                allStored.TryGetValue(kv.Key, out stored);
                MyFixedPoint total = queued + stored;
                MyFixedPoint gap = kv.Value - total;
                Echo($"Found total {total}, queued {queued}, requested {kv.Value}");
                while (gap >= workUnit && queued <= workUnit * (maxWorkUnits - 1))
                {
                    ++queueStep;
                    MyDefinitionId blueprint = ToBlueprintDefinition(kv.Key);
                    // load balance between all assemblers that can build the blueprint
                    List<IMyAssembler> buildable = primaries.FindAll(a => a.CanUseBlueprint(blueprint));
                    if (buildable.Count() == 0)
                    {
                        Echo($"Warning no Assembler found for {blueprint.ToString()}");
                        break;
                    }
                    IMyAssembler assembler = buildable[queueStep % buildable.Count()];
                    Echo($"Queuing {workUnit} of {kv.Key.ToString()} to {assembler.CustomName} as {blueprint.ToString()}");
                    assembler.AddQueueItem(ToBlueprintDefinition(kv.Key), workUnit);
                    gap += workUnit;
                    queued += workUnit;
                }

            }
        }
    }
}
