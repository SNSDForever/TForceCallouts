using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Engine.Scripting.Entities;

namespace TaskForceCallouts.Callouts
{
    [CalloutInfo("TrackTerrorist", CalloutProbability.VeryLow)]
    public class TrackTerrorist : Callout
    {
        private static Ped Commander;
        private static Ped Suspect;
        private static Ped[] secs = new Ped[11];
        private Vector3[] pedvecs = new Vector3[11];
        private static Vehicle[] vecs = new Vehicle[7];
        private Vector3[] vecvecs = new Vector3[7];
        private Vehicle SuspectVehicle;
        private static Vector3 SuspectSpawnPoint = new Vector3(1967.569f, -726.6521f, 88.9337f);
        private Vector3 EndPoint = new Vector3(2350.525f, 3134.466f, 47.79085f);
        private Vector3 CommanderPos = new Vector3(2502.131f, -432.7624f, 92.99282f);
        private Vector3[] TerVecs = new Vector3[32];
        private Ped[] Ters = new Ped[32];
        private LHandle Pursuit;
        private bool PursuitCreated = false;
        private Blip SuspectBlip;
        private Blip CommanderBlip;
        private Vector3 valvec = new Vector3(2398.642f, 3131.618f, 52.42918f);
        private Vehicle valk;
        private Vector3 Bar1vec = new Vector3(2336.373f, 3154.944f, 47.95555f);
        private Vector3 Bar2vec = new Vector3(2333.461f, 3085.676f, 47.8271f);
        private Vector3 Bar3vec = new Vector3(2342.214f, 3089.885f, 47.84767f);
        private Vehicle Bar1;
        private Vehicle Bar2;
        private Vehicle Bar3;
        private Vector3 Ha1vec = new Vector3(2343.484f, 3141.844f, 47.82887f);
        private Vector3 Ha2vec = new Vector3(2349.45f, 3123.584f, 47.82982f);
        private Vector3 Ha3vec = new Vector3(2356.417f, 3124.151f, 47.82552f);
        private Vehicle Ha1;
        private Vehicle Ha2;
        private Vehicle Ha3;
        private Vector3 apos1 = new Vector3(2339.939f, 3052.031f, 47.91319f);
        private Vector3 apos2 = new Vector3(2338.203f, 3039.189f, 47.84728f);
        private Vehicle APC1;
        private Vehicle APC2;
        private Vector3 BBvec = new Vector3(2445.434f, 3103.712f, 47.27837f);
        private Vehicle BB;
        private Vector3 Rockvec1 = new Vector3(2391.15f, 3081.555f, 47.86066f);
        private Vector3 Rockvec2 = new Vector3(2384.938f, 3061.247f, 47.88268f);
        private Vector3 Rockvec3 = new Vector3(2377.246f, 3058.771f, 48.03436f);
        private Vehicle rock1, rock2, rock3;
        private static Ped[] Backups = new Ped[20];
        private static Vehicle[] BackupVs = new Vehicle[20];

        private static String[] conversation =
        {
            "We are informed that the spy inside Fort Zancudo is now on freeway",
            "Also, he's about to attck Fort zancudo and attack General Ford",
            "We want you to track him down with unmarked vehicle till he reach the base",
            "When you reach the base, call for backups. SWAT team is on your back",
            "Police Maverick will follow you above"
        };

        private static String[] SpeechScan =
        {
            "CS_1", "CS_2", "CS_3", "CS_4", "CS_5"
        };

        private static void spawnBackUps()
        {
            Vector3 per = new Vector3(0, 0, 0);
            for (int i = 0; i < Backups.Length; i++)
            {
                if (i % 4 == 0)
                {
                    BackupVs[i] = new Vehicle("FBI2", World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(150f)));
                    Backups[i] = new Ped("s_m_y_swat_01", per, 0f);
                    Backups[i].Inventory.GiveNewWeapon("w_ar_carbinerifle", 1000, true);
                    Backups[i].WarpIntoVehicle(BackupVs[i], 1);
                }
                else if (i % 4 == 1)
                {
                    Backups[i] = new Ped("s_m_y_swat_01", per, 0f);
                    Backups[i].Inventory.GiveNewWeapon("w_ar_carbinerifle", 1000, true);
                    Backups[i].WarpIntoVehicle(BackupVs[i - 1], 1);
                }
                else if (i % 4 == 2)
                {
                    Backups[i] = new Ped("s_m_y_swat_01", per, 0f);
                    Backups[i].Inventory.GiveNewWeapon("w_ar_carbinerifle", 1000, true);
                    Backups[i].WarpIntoVehicle(BackupVs[i - 2], 1);
                }
                else
                {
                    Backups[i] = new Ped("s_m_y_swat_01", per, 0f);
                    Backups[i].Inventory.GiveNewWeapon("w_ar_carbinerifle", 1000, true);
                    Backups[i].WarpIntoVehicle(BackupVs[i - 3], 1);
                }
            }
            for (int i = 0; i < BackupVs.Length; i += 4)
            {
                BackupVs[i].IsPersistent = true;
            }
            for (int i = 0; i < Backups.Length; i++)
            {
                Backups[i].IsPersistent = true;
            }
            Vector3[] buarrives = new Vector3[20];
            for (int i = 0; i < BackupVs.Length; i += 4)
            {
                buarrives[i] = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(10f));
                Backups[i].Tasks.DriveToPosition(buarrives[i], 60f, VehicleDrivingFlags.Emergency);
            }

            if (BackupVs[0].Position == buarrives[0])
            {
                for (int j = 0; j < 4; j++)
                {
                    Backups[j].Tasks.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                }
            }
        }

        private static void DeleteNOOSE()
        {
            if (Commander.Exists())
            {
                Commander.Delete();
            }
            for (int i = 0; i < secs.Length - 1; i++)
            {
                secs[i].Delete();
            }
            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i].Delete();
            }
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            ShowCalloutAreaBlipBeforeAccepting(CommanderPos, 10000f);
            AddMinimumDistanceCheck(10f, CommanderPos);

            CalloutMessage = "Track Terorist";
            CalloutPosition = CommanderPos;

            //Functions.PlayScannerAudioUsingPosition("Classified_Task_from_Noose_Headquarters Respond_immediately", CommanderPos);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            CommanderBlip = new Blip(CommanderPos);
            CommanderBlip.Color = System.Drawing.Color.Black;
            CommanderBlip.EnableRoute(System.Drawing.Color.Black);
            Commander = new Ped("s_m_m_marine_02", CommanderPos, 159.3015f)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };

            pedvecs[0] = new Vector3(2505.183f, -434.0856f, 92.99282f);
            pedvecs[1] = new Vector3(2501.618f, -431.4405f, 92.99282f);
            pedvecs[2] = new Vector3(2499.614f, -431.3284f, 92.99284f);
            pedvecs[3] = new Vector3(2498.765f, -458.2293f, 92.99288f);
            pedvecs[4] = new Vector3(2487.147f, -468.7034f, 93.09234f);
            pedvecs[5] = new Vector3(2462.916f, -438.917f, 92.99331f);
            pedvecs[6] = new Vector3(2467.216f, -433.301f, 92.99276f);
            pedvecs[7] = new Vector3(2467.217f, -435.323f, 92.99276f);
            pedvecs[8] = new Vector3(2463.92f, -431.0506f, 92.99276f);
            pedvecs[9] = new Vector3(2458.222f, -433.7596f, 92.99314f);
            pedvecs[10] = new Vector3(2517.226f, -423.2008f, 118.0279f);

            secs[0] = new Ped("s_m_y_swat_01", pedvecs[0], 151.4705f);
            secs[1] = new Ped("s_m_y_swat_01", pedvecs[1], 174.9196f);
            secs[2] = new Ped("s_m_y_swat_01", pedvecs[2], 171.8672f);
            secs[3] = new Ped("s_m_m_armoured_01", pedvecs[3], 52.7207f);
            secs[4] = new Ped("s_m_m_armoured_01", pedvecs[4], 317.0834f);
            secs[5] = new Ped("s_m_y_swat_01", pedvecs[5], 225.0199f);
            secs[6] = new Ped("s_m_y_swat_01", pedvecs[6], 209.9155f);
            secs[7] = new Ped("s_m_y_swat_01", pedvecs[7], 209.4709f);
            secs[8] = new Ped("s_m_y_swat_01", pedvecs[8], 31.51678f);
            secs[9] = new Ped("s_m_y_swat_01", pedvecs[9], 41.02285f);
            secs[10] = new Ped("s_m_y_swat_01", pedvecs[10], 91.96522f);

            for (int i = 0; i < secs.Length; i++)
            {
                secs[i].IsPersistent = true;
                secs[i].BlockPermanentEvents = true;
            }

            vecvecs[0] = new Vector3(2507.223f, -450.903f, 92.61516f);
            vecvecs[1] = new Vector3(2490.807f, -438.1205f, 92.61532f);
            vecvecs[2] = new Vector3(2487.822f, -437.635f, 92.64066f);
            vecvecs[3] = new Vector3(2463.575f, -434.1258f, 92.6488f);
            vecvecs[4] = new Vector3(2520.533f, -480.4149f, 92.85534f);
            vecvecs[5] = new Vector3(2522.016f, -458.8109f, 92.82905f);
            vecvecs[6] = new Vector3(2511.784f, -426.8316f, 118.5766f);

            vecs[0] = new Vehicle("FBI2", vecvecs[0], 48.77747f);
            vecs[1] = new Vehicle("FBI2", vecvecs[1], 183.1994f);
            vecs[2] = new Vehicle("FBI", vecvecs[2], 182.1779f);
            vecs[3] = new Vehicle("RIOT", vecvecs[3], 217.3658f);
            vecs[4] = new Vehicle("RIOT2", vecvecs[4], 284.8067f);
            vecs[5] = new Vehicle("RIOT2", vecvecs[5], 282.636f);
            vecs[6] = new Vehicle("POLMAV", vecvecs[6], 43.36565f);
            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i].IsPersistent = true;
            }

            SuspectVehicle = new Vehicle("GAUNTLET", SuspectSpawnPoint, 306.53f);
            Vector3 temp = new Vector3(0, 0, 0);
            Suspect = new Ped("s_m_y_armymech_01", temp, 0f);
            Suspect.WarpIntoVehicle(SuspectVehicle, 1);
            Suspect.IsPersistent = true;
            SuspectVehicle.IsPersistent = true;
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            Boolean IsSpeechFinished = false;
            if (!IsSpeechFinished && Game.LocalPlayer.Character.DistanceTo(CommanderPos) < 8f)
            {
                Game.DisplayNotification("Press Y to talk with the NOOSE Commander");
            }
            for (int i = 0; i < conversation.Count(); i++)      
            {
                while (!Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                    GameFiber.Yield();

                Game.DisplaySubtitle(conversation[i]);
                GameFiber.Sleep(1000);
                Functions.PlayScannerAudio(SpeechScan[i]);
            }
            IsSpeechFinished = true;

            if (IsSpeechFinished == true)
            {
                secs[10].Tasks.EnterVehicle(vecs[6], 1, EnterVehicleFlags.None);
                Vector3 Tmav = new Vector3(0f, 0f, 80f);
                secs[10].Tasks.ChaseWithHelicopter(Game.LocalPlayer.Character, Tmav);

                SuspectBlip = Suspect.AttachBlip();
                SuspectBlip.IsFriendly = false;
                Suspect.Tasks.DriveToPosition(EndPoint, 60f, VehicleDrivingFlags.Normal);
            }

            if (!PursuitCreated && Game.LocalPlayer.Character.DistanceTo(Suspect) < 30f && Game.LocalPlayer.Character.DistanceTo(Suspect) >= 20f)
            {
                Game.DisplaySubtitle("Don't get too close to suspect!");
            }
            else if (!PursuitCreated && Game.LocalPlayer.Character.DistanceTo(Suspect) < 10f)
            {
                Pursuit = Functions.CreatePursuit();
                Game.DisplaySubtitle("Follow him till he reach the base! DO NOT DISTURB HIM!");
                Suspect.Tasks.DriveToPosition(EndPoint, 100f, VehicleDrivingFlags.Emergency);
            }

            Vector3 BaseLocation = new Vector3(2391.15f, 3081.555f, 47.86066f);
            if (BaseLocation.DistanceTo(Game.LocalPlayer.Character.Position) < 100f)
            {
                TerVecs[0] = new Vector3(2339.8f, 3129.015f, 48.20871f);
                Ters[0] = new Ped(0x7a05fa59, TerVecs[0], 337.8885f);
                TerVecs[1] = new Vector3(2341.704f, 3128.805f, 48.20871f);
                Ters[1] = new Ped(0x7a05fa59, TerVecs[1], 331.1746f);
                TerVecs[2] = new Vector3(2340.149f, 3140.876f, 48.20823f);
                Ters[2] = new Ped(0x7a05fa59, TerVecs[2], 127.9696f);
                TerVecs[3] = new Vector3(2335.009f, 3130.267f, 48.19203f);
                Ters[3] = new Ped(0xb3f3ee34, TerVecs[3], 96.67557f);
                TerVecs[4] = new Vector3(2321.998f, 3125.913f, 48.14739f);
                Ters[4] = new Ped(0xb3f3ee34, TerVecs[4], 91.01817f);
                TerVecs[5] = new Vector3(2319.701f, 3126.135f, 48.10742f);
                Ters[5] = new Ped(0x7a05fa59, TerVecs[5], 74.91132f);
                TerVecs[6] = new Vector3(2318.211f, 3127.677f, 48.10079f);
                Ters[6] = new Ped(0xb3f3ee34, TerVecs[6], 51.43881f);
                TerVecs[7] = new Vector3(2343.425f, 3111.177f, 48.20897f);
                Ters[7] = new Ped(0xb3f3ee34, TerVecs[7], 285.0285f);
                TerVecs[8] = new Vector3(2327.604f, 3080.094f, 48.09248f);
                Ters[8] = new Ped(0xb3f3ee34, TerVecs[8], 174.2533f);
                TerVecs[9] = new Vector3(2327.156f, 3076.347f, 48.0293f);
                Ters[9] = new Ped(0x7a05fa59, TerVecs[9], 174.1915f);
                TerVecs[10] = new Vector3(2326.77f, 3072.597f, 47.88791f);
                Ters[10] = new Ped(0x7a05fa59, TerVecs[10], 174.1547f);
                TerVecs[11] = new Vector3(2359.828f, 3111.66f, 48.20872f);
                Ters[11] = new Ped(0x7a05fa59, TerVecs[11], 344.3957f);
                TerVecs[12] = new Vector3(2359.925f, 3112.162f, 48.20892f);
                Ters[12] = new Ped(0x7a05fa59, TerVecs[12], 350.2567f);
                TerVecs[13] = new Vector3(2360.113f, 3113.512f, 48.20892f);
                Ters[13] = new Ped(0xb3f3ee34, TerVecs[13], 352.0925f);
                TerVecs[14] = new Vector3(2404.439f, 3127.453f, 48.15347f);
                Ters[14] = new Ped(0x7a05fa59, TerVecs[14], 351.4263f);
                TerVecs[15] = new Vector3(2404.541f, 3129.613f, 48.15307f);
                Ters[15] = new Ped(0xb3f3ee34, TerVecs[15], 358.8074f);
                TerVecs[16] = new Vector3(2417.235f, 3132.939f, 48.18722f);
                Ters[16] = new Ped(0x7a05fa59, TerVecs[16], 249.7016f);
                TerVecs[17] = new Vector3(2418.303f, 3132.53f, 48.18659f);
                Ters[17] = new Ped(0x7a05fa59, TerVecs[17], 248.8401f);
                TerVecs[18] = new Vector3(2418.909f, 3132.295f, 48.18205f);
                Ters[18] = new Ped(0x7a05fa59, TerVecs[18], 248.8235f);
                TerVecs[19] = new Vector3(2419.778f, 3131.804f, 48.17218f);
                Ters[19] = new Ped(0xb3f3ee34, TerVecs[19], 215.806f);
                TerVecs[20] = new Vector3(2348.745f, 3061.8f, 48.15229f);
                Ters[20] = new Ped(0x7a05fa59, TerVecs[20], 167.6354f);
                TerVecs[21] = new Vector3(2349.083f, 3060.483f, 48.15235f);
                Ters[21] = new Ped(0x7a05fa59, TerVecs[21], 174.1753f);
                TerVecs[22] = new Vector3(2374.485f, 3060.779f, 48.15276f);
                Ters[22] = new Ped(0xb3f3ee34, TerVecs[22], 174.4138f);
                TerVecs[23] = new Vector3(2374.373f, 3058.497f, 48.15263f);
                Ters[23] = new Ped(0x7a05fa59, TerVecs[23], 177.3636f);
                TerVecs[24] = new Vector3(2403.358f, 3052.894f, 48.12256f);
                Ters[24] = new Ped(0x7a05fa59, TerVecs[24], 301.0558f);
                TerVecs[25] = new Vector3(2405.572f, 3056.084f, 48.15279f);
                Ters[25] = new Ped(0xb3f3ee34, TerVecs[25], 1.373306f);
                TerVecs[26] = new Vector3(2405.519f, 3057.031f, 48.15287f);
                Ters[26] = new Ped(0x7a05fa59, TerVecs[26], 4.601843f);
                TerVecs[27] = new Vector3(2419.602f, 3069.834f, 49.57166f);
                Ters[27] = new Ped(0xb3f3ee34, TerVecs[27], 359.0211f);
                TerVecs[28] = new Vector3(2419.615f, 3071.597f, 49.58511f);
                Ters[28] = new Ped(0xb3f3ee34, TerVecs[28], 358.4223f);
                TerVecs[29] = new Vector3(2419.635f, 3072.341f, 49.59034f);
                Ters[29] = new Ped(0xb3f3ee34, TerVecs[29], 358.2909f);
                TerVecs[30] = new Vector3(2436.623f, 3117.492f, 48.09231f);
                Ters[30] = new Ped(0x7a05fa59, TerVecs[30], 291.501f);
                TerVecs[31] = new Vector3(2436.781f, 3100.537f, 48.10157f);
                Ters[31] = new Ped(0xb3f3ee34, TerVecs[31], 326.464f);
                valk = new Vehicle("VALKYRIE", valvec, 63.46453f);
                Bar1 = new Vehicle("BARRAGE", Bar1vec, 109.8383f);
                Bar2 = new Vehicle("BARRAGE", Bar2vec, 53.84727f);
                Bar3 = new Vehicle("BARRAGE", Bar3vec, 80.02969f);
                Ha1 = new Vehicle("HALFTRACK", Ha1vec, 167.8275f);
                Ha2 = new Vehicle("HALFTRACK", Ha2vec, 261.7653f);
                Ha3 = new Vehicle("HALFTRACK", Ha3vec, 351.8081f);
                APC1 = new Vehicle("APC", apos1, 271.8866f);
                APC2 = new Vehicle("APC", apos2, 266.9066f);
                BB = new Vehicle("BARRAGE", BBvec, 286.3922f);
                rock1 = new Vehicle("CHERNOBOG", Rockvec1, 2.197593f);
                rock2 = new Vehicle("CHERNOBOG", Rockvec2, 6.571267f);
                rock3 = new Vehicle("CHERNOBOG", Rockvec3, 354.8148f);

                for (int i = 0; i < Ters.Length; i++)
                {
                    Ters[i].Inventory.GiveNewWeapon("w_ar_carbinerifle", 1000, true);
                }
            }

            if (!PursuitCreated && SuspectVehicle.Position == EndPoint)
            {
                DeleteNOOSE();
                Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.None);
                Suspect.Tasks.Wander();
                secs[10].Tasks.Wander();
                for (int i = 0; i < Ters.Length; i++)
                {
                    Ters[i].Tasks.StandStill(3000);
                }
                Game.DisplayNotification("Press Y for BackUps or you can use your own BackUp plugins");
                if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                {
                    spawnBackUps();
                }
            }
            else if (PursuitCreated && SuspectVehicle.Position == EndPoint)
            {
                DeleteNOOSE();
                Suspect.Tasks.LeaveVehicle(LeaveVehicleFlags.BailOut);
                Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                secs[10].Tasks.Wander();
                for (int i = 0; i < Ters.Length; i++)
                {
                    Ters[i].Tasks.FireWeaponAt(Game.LocalPlayer.Character, 3000, FiringPattern.FullAutomatic);
                   
                }
                Game.DisplayNotification("Press Y for BackUps or you can use your own BackUp plugins");

                if (Game.IsKeyDown(System.Windows.Forms.Keys.Y))
                {
                    spawnBackUps();
                }

            }
           

        }

        public override void End()
        {
            if (Suspect.Exists())
            {
                Suspect.Delete();
            }

            for(int i=0; i<secs.Length; i++)
            {
                if (secs[i].Exists())
                {
                    secs[i].Delete();
                }
            }

            for(int i=0; i<vecs.Length; i++)
            {
                if (vecs[i].Exists())
                {
                    vecs[i].Delete();
                }
            }

            if (SuspectVehicle.Exists())
            {
                SuspectVehicle.Delete();
            }

            if (SuspectBlip.Exists())
            {
                SuspectBlip.Delete();
            }

            for(int i=0; i<Ters.Length; i++)
            {
                if (Ters[i].Exists())
                {
                    Ters[i].Delete();
                }
            }

            if (CommanderBlip.Exists())
            {
                CommanderBlip.Delete();
            }

            for(int i=0; i<Backups.Length; i++)
            {
                if (Backups[i].Exists())
                {
                    Backups[i].Dismiss();
                }
            }

        




        }

    }
}
