using PSD.Base;
using PSD.Base.Card;
using System.Linq;

namespace PSD.PSDGamepkg
{
    public partial class XI
    {
        private int[] mode00heroes = new int[] {
            //10602, 17005, 10107, 10404, 10501, 10504
            //10102, 10303, 10605, 10402, 10305, 10504
            //10303, 10105, 10605, 10402, 10305, 10102
            //17002, 10105, 10605, 10606, 10402, 10608
            //10101, 10401, 10206, 10501, 17004, 10502
            //10303, 10606, 17006, 10305, 17002, 10501
            //10102, 10606, 10602, 10306, 17001, 10501
            //15005, 17004, 10605, 10306, 17001, 10503
            //15005, 17005, 10501, 10306, 17003, 10503
            //10302, 10101, 10608, 10203, 10306, 10206
            //10604, 10105, 15004, 10402, 10101, 10501
            //15001, 10302, 10505, 10102, 10104, 10401
            //17015, 17016, 10602, 10101, 10104, 10604
            //17009, 17014, 10501, 17013, 17015, 17017
            //17018, 17012, 10601, 17010, 17009, 17016
            //17008, 17010, 17017, 17012, 17013, 17009
            //17008, 17004, 17017, 10505, 17013, 17009
            //17008, 17009, 10305, 17012, 10605, 17010
            //10206, 10305, 17011, 17009, 10306, 17010
            //15005, 17004, 15004, 10101, 17015, 10106
            //19001, 17010, 17009, 10603, 10601, 17016
            //10302, 17010, 19008, 10603, 10206, 17016
            //17015, 17010, 10605, 10603, 10501, 17016
            //10404, 10302, 17015, 10603, 15005, 17016
            //19012, 17013, 17015, 10603, 15005, 17016
            //10403, 17013, 15009, 10302, 15005, 17016
            //19001, 10302, 10102, 10606, 10203, 10608
            //10603, 19018, 10605, 10501, 15003, 10505
            //17022, 19006, 10605, 10501, 15003, 10505
            //19009, 17022, 19001, 19011, 10107, 10302
            //19009, 10105, 19013, 17010, 19017, 19010
            //10602, 17022, 19002, 10104, 19009, 19006
            //17007, 17020, 19002, 10104, 17011, 19006
            //10102, 10201, 17007, 10104, 17011, 19006
            //17021, 10504, 19002, 10104, 17011, 19006
            //17003, 17022, 10601, 19011, 10604, 19013
            //10203, 15008, 10302, 10603, 10604, 17011
            //19016, 17007, 17020, 19011, 19008, 10601
            //19014, 17005, 17020, 19011, 19008, 19016
            //10303, 10105, 10102, 10601, 17018, 19013
            //19011, 19003, 10601, 10502, 17018, 19013
            //10102, 17028, 17025, 10206, 10504, 19011
            //17027, 17028, 17005, 17025, 10608, 19011
            //10303, 10404, 10602, 17017, 10403, 10504
            //10302, 17012, 17005, 17022, 10605, 17028
            //10105, 10608, 10605, 10303, 19010, 17002
            //19014, 10102, 10501, 10303, 19010, 19016
            //17041, 19020, 10606, 18004, 10303, 19006
            //17008, 15008, 10201, 17037, 19021, 19006
            //15002, 17007, 10105, 17021, 19018, 10502
            //17039, 19018, 17027, 17022, 17034, 17042
            //17029, 17036, 17027, 17045, 17043, 17040
            //17014, 19007, 10305, 17002, 19013, 17028
            //17023, 10305, 15002, 19016, 15001, 17004
            //17029, 10206, 17005, 17040, 17027, 19003
            //19014, 17029, 19010, 19004, 19015, 17032
            //17035, 19002, 17044, 17019, 10605, 17036
            //17044, 19002, 17026, 10404, 17036, 10502
            //19014, 19006, 19018, 19011, 17035, 19010
            //17024, 15005, 10501, 17039, 19011, 10605
            //10501, 15005, 19016, 19007, 10603, 17029
            //10602, 17040, 10203, 10503, 17007, 10603,
            //10603, 10605, 17030, 17027, 17029, 17034,
            17024, 10107, 10605, 10106, 10504, 17001
        };

        private void DebugCondition()
        {
            //RaiseGMessage("G0HQ,1,0,1,10,56,37,4,47,48,49,50,51,52,53,54,55,12");
            //RaiseGMessage("G0HQ,2,0,1,40,42,38,14,11");
            //RaiseGMessage("G0HQ,3,0,1,3");
            //RaiseGMessage("G0HQ,2,0,1,44");
            //RaiseGMessage("G0HQ,1,0,1,10,11,12,52,36,4,1,33");
            //RaiseGMessage("G0HQ,2,0,1,48,34,7");
            //RaiseGMessage("G0HQ,3,0,1,34");
            //RaiseGMessage("G0HQ,1,0,1,54,49");
            //RaiseGMessage("G0HQ,2,3,0,44,8,16");
            //RaiseGMessage("G0HQ,2,6,0,28,42,4");
            //RaiseGMessage("G0HQ,2,5,0,24,31,45");
            //RaiseGMessage("G0HQ,2,1,0,47,48");//7,1,13
            //RaiseGMessage("G0HQ,2,2,0,15,32");//54
            //RaiseGMessage("G0HQ,2,4,0,51,26");//33
            //RaiseGMessage("G0HQ,2,1,0,0,17");

            //RaiseGMessage("G0HQ,2,2,0,0,88,44");
            //RaiseGMessage("G0HQ,2,5,0,0,77");
            //RaiseGMessage("G0IJ,5,0,4");
            //RaiseGMessage("G0IX,5,0,2");
            //garden[2].Weapon = 51;
            //garden[1].Pets[3] = 16;
            //garden[1].Pets[4] = 20;
            //garden[2].Pets[0] = 4;
            //Board.MonPiles.PushBack(13);
            //Board.MonPiles.PushBack(1011);
            //Board.MonPiles.PushBack(8);
            //RaiseGMessage("G0HC,0,1,19");
            //Board.MonPiles.PushBack(1022);
            //Board.MonPiles.PushBack(1041);
            //Board.MonPiles.PushBack(57);
            //Board.MonPiles.PushBack(1059);
            //Board.MonPiles.PushBack(1068);
            //Board.MonPiles.PushBack(26);
            //Board.MonPiles.PushBack(1);
            //Board.MonPiles.PushBack(17);
            //Board.MonPiles.PushBack(14);
            //Board.MonPiles.PushBack(46);
            //Board.MonPiles.PushBack(41);
            //Board.MonPiles.PushBack(13);
            //Board.MonPiles.PushBack(49);
            //Board.MonPiles.PushBack(24);
            //Board.MonPiles.PushBack(25);
            //Board.MonPiles.PushBack(24);
            //Board.MonPiles.PushBack(23);
            //Board.MonPiles.PushBack(22);
            //Board.MonPiles.PushBack(47);
            //Board.MonPiles.PushBack(49);
            //Board.MonPiles.PushBack(1053);
            //Board.MonPiles.PushBack(1001);
            //Board.MonPiles.PushBack(32);
            //Board.MonPiles.PushBack(1103);
            //Board.MonPiles.PushBack(1105);
            //Board.MonPiles.PushBack(1108);
            //Board.MonPiles.PushBack(1001);
            //Board.MonPiles.PushBack(1106);
            //Board.MonPiles.PushBack(1104);
            //Board.MonPiles.PushBack(1103);
            //Board.MonPiles.PushBack(1107);
            //Board.MonPiles.PushBack(1109);
            //Board.MonPiles.PushBack(1106);
            //Board.EvePiles.PushBack(35);
            ////Board.EvePiles.PushBack(1);
            Board.EvePiles.PushBack(31);
            //Board.EvePiles.PushBack(42);
            //Board.EveDises.Add(Board.EvePiles.Dequeue());
            //Board.EveDises.Add(Board.EvePiles.Dequeue());
            //Board.EvePiles.PushBack(52);
            //Board.EvePiles.PushBack(27);
            //Board.EvePiles.PushBack(30);
            //Board.EvePiles.PushBack(31);
            //Board.EvePiles.PushBack(9);
            //Board.RestNPCPiles.PushBack(1068);
            //oard.RestNPCPiles.PushBack(1077);
            Board.RestNPCPiles.PushBack(1102);
            // Board.HeroPiles.PushBack(19003);
            //Board.EvePiles.PushBack(31);
            //while (Board.MonPiles.Count > 0)
            //    Board.MonPiles.Dequeue();
            //Board.MonPiles.PushBack(74);
            //Board.MonPiles.PushBack(46);
            //Board.MonPiles.PushBack(67);
            //Board.MonPiles.PushBack(76);
            //Board.MonPiles.PushBack(65);
            //Board.MonPiles.PushBack(13);
            //Board.MonPiles.PushBack(1019);
            //Board.MonPiles.PushBack(1022);
            //Board.MonPiles.PushBack(7);
            //Board.MonPiles.PushBack(1015);
            //Board.MonPiles.PushBack(28);
            //Board.MonPiles.PushBack(1013);
            //Board.MonPiles.PushBack(1002);
            //Board.MonPiles.PushBack(1012);
            //Board.MonPiles.PushBack(40);
            //Board.MonPiles.PushBack(43);
            //Board.RestNPCPiles.PushBack(1001);
            //Board.MonPiles.PushBack(18);
            //Board.MonPiles.PushBack(62);
            //Board.MonPiles.PushBack(1003);
            Board.MonPiles.PushBack(19);
            Board.MonPiles.PushBack(8);
            //Board.MonPiles.PushBack(1077);
            //Board.MonPiles.PushBack(23);
            //Board.MonPiles.PushBack(2);
            //Board.MonPiles.PushBack(37);
            //Board.MonPiles.PushBack(1025);
            //Board.RestNPCPiles.PushBack(1025);
            //List<ushort> mons = new List<ushort>()
            //{
            //    31,32,33,34,35,36,37,38,39,40
            //};
            //mons.Shuffle();
            //foreach (ushort mon in mons)
            //    Board.MonPiles.PushBack(mons);

            //for (int i = 0; i < 28; ++i)
            //    Board.MonPiles.Dequeue();
            //Board.MonPiles.PushBack(1001);
            //Board.MonPiles.PushBack(29);
            //Board.MonPiles.PushBack(54);
            //Board.MonPiles.PushBack(33);
            //Board.MonPiles.PushBack(13);
            //Board.MonPiles.PushBack(1110);
            //Board.MonPiles.PushBack(8);
            //Board.MonPiles.PushBack(44);
            //Board.MonPiles.PushBack(50);
            //Board.MonPiles.PushBack(49);
            //Board.RestNPCPiles.PushBack(1075);
            //Board.MonPiles.PushBack(1112);
            //Board.MonPiles.PushBack(1111);
            //Board.MonPiles.Dequeue();
            //Board.MonPiles.PushBack(49);
            //Board.MonPiles.PushBack(47);
            //Board.MonDises.Add(4);
            //Board.MonPiles.PushBack(6);
            //Board.MonPiles.PushBack(1040);
            //Board.MonPiles.PushBack(1109);
            //Board.MonPiles.PushBack(1031);
            //Board.MonPiles.PushBack(1);
            //Board.MonPiles.PushBack(50);
            //Board.RestMonPiles.PushBack(2);
            //Board.RestMonPiles.PushBack(3);
            //Board.RestMonPiles.PushBack(7);
            //Board.RestMonPiles.PushBack(17);
            //Board.TuxPiles.PushBack(76);
            //Board.TuxPiles.PushBack(75);
            //Board.TuxPiles.PushBack(1);
            //RaiseGMessage("G0HQ,2,1,0,52,10,11");
            //RaiseGMessage("G0HQ,2,1,0,47,49,10,11,12");
            //Board.MonPiles.PushBack(54);
            //RaiseGMessage("G0IJ,1,0,3");
            //Board.MonPiles.PushBack(1016);
            //Board.MonPiles.PushBack(6);
            //Board.TuxPiles.Dequeue(30); // 56 - 18 = 38
            //Board.TuxPiles.PushBack(50);
            //RaiseGMessage("G0IJ,6,0,1");
            //RaiseGMessage("G0HQ,2,1,0,10,47,53,6");
            //RaiseGMessage("G0HQ,2,3,0,20,26");
            //RaiseGMessage("G0HQ,2,1,0,0,5,100,81,3");
            //Board.RestNPCPiles.PushBack(1057);
            //RaiseGMessage("G0HQ,2,1,0,0,49,60");
            //RaiseGMessage("G0HQ,2,1,0,0,50,110,71,2,96");
            //RaiseGMessage("G0HQ,2,1,0,0,110,100");
            // RaiseGMessage(new Artiad.EquipStandard()
            // {
            //     Who = 2,
            //     FromSky = true,
            //     SlotAssign = true,
            //     Cards = new ushort[] { 50, 51, 111, 110 }
            // }.ToMessage());
            // RaiseGMessage(new Artiad.EquipStandard()
            // {
            //     Who = 1,
            //     FromSky = true,
            //     Cards = new ushort[] { 48 }
            // }.ToMessage());
            //RaiseGMessage("G0HQ,2,3,0,0,84");
            //RaiseGMessage("G0HQ,2,1,0,0,74,98,99,100");
            //RaiseGMessage("G0HQ,2,1,0,0,123,101");
            //RaiseGMessage("G0HQ,2,1,0,0,90,92");
            //RaiseGMessage("G0HQ,2,1,0,0,101,127");
            //RaiseGMessage("G0HQ,2,4,0,0,49,2");
            //RaiseGMessage("G0HQ,2,1,0,0,77,99");
            //RaiseGMessage("G0HQ,2,6,0,0,101");
            //RaiseGMessage("G0HQ,2,1,0,47,50,49,5,63,8,69");
            //RaiseGMessage("G0HQ,2,3,0,0,37");
            //RaiseGMessage("G0HQ,2,2,0,0,77,79");
            //RaiseGMessage("G0HQ,2,5,0,0,27,28,29");
            //RaiseGMessage("G0HQ,2,1,0,0,95,53,109,10,92,88");
            //RaiseGMessage("G0HQ,2,3,0,0,108");
            //RaiseGMessage("G0HQ,2,5,0,0,137");
            //RaiseGMessage("G0HQ,2,2,0,0,103");
            //RaiseGMessage("G0HQ,2,4,0,0,47,48,52");
            //RaiseGMessage("G0HQ,2,1,0,0,95,1");
            //RaiseGMessage("G0HQ,2,2,0,0,136");
            //RaiseGMessage("G0HQ,2,1,0,0,124,101,117,71");
            //RaiseGMessage("G0HQ,2,2,0,0,102,69");
            //RaiseGMessage("G0HQ,2,1,0,61,64,73,74,75,76,65,17,69,71,10,70");
            //RaiseGMessage("G0IJ,3,0,1");
            //RaiseGMessage("G0IJ,3,0,1");
            //RaiseGMessage("G0IX,5,0,1");
            //RaiseGMessage("G0HQ,2,1,0,0,93,94,9,18,96,5,2,71,72,49,60,11");
            //RaiseGMessage("G0HQ,2,1,0,0,30,33");
            //RaiseGMessage("G0HQ,2,1,0,0,10,37,50");
            //RaiseGMessage("G0HQ,2,4,0,0,33,35");
            RaiseGMessage("G0HQ,2,1,0,0,100");
            //RaiseGMessage("G0HQ,2,2,0,0,90,89");
            //RaiseGMessage("G0HQ,2,2,0,0,96,59");
            //RaiseGMessage("G0HQ,2,1,0,0,96,18");
            //RaiseGMessage("G0HQ,2,5,0,0,127");
            //RaiseGMessage("G0HQ,2,1,0,0,71,37,95");
            //RaiseGMessage("G0HQ,2,1,0,0,50,51,110,111,109");
            //RaiseGMessage("G0HQ,2,1,0,0,55,73,95,1,5");
            //RaiseGMessage("G0HQ,2,2,0,0,39,40,41,42");
            //RaiseGMessage("G0HQ,2,3,0,0,48,53");
            //RaiseGMessage("G0HQ,2,4,0,0,60,90");
            //RaiseGMessage("G0HQ,2,4,0,0,40,70");
            //RaiseGMessage("G0HQ,2,2,0,0,90,34,89,88,95");
            //RaiseGMessage("G0HQ,2,1,0,0,95,88,10");
            //RaiseGMessage("G0HQ,2,1,0,0,10,11");
            //RaiseGMessage("G0HQ,2,1,0,10,38,39");
            //RaiseGMessage("G0HQ,2,1,0,0,49,20");
            //RaiseGMessage("G0HQ,2,1,0,1,47,48,49,51,52");
            //RaiseGMessage("G0HQ,2,1,0,71,72,10,79,8");
            //RaiseGMessage("G0HQ,2,2,0,55,84");
            //RaiseGMessage("G0HQ,2,2,0,0,90");
            //RaiseGMessage("G0HQ,2,1,0,0,132,70");
            //RaiseGMessage("G0HQ,2,4,0,50,32,1,2");
            //RaiseGMessage("G0HQ,2,5,0,0,70");
            //RaiseGMessage("G0HQ,2,6,0,0,52");
            //RaiseGMessage("G0HQ,2,1,0,51,37,10,53,11,40,16,18,25");
            //RaiseGMessage("G0HQ,2,2,0,48,49,13,14,34");
            //RaiseGMessage("G0HQ,2,3,0,1,52");
            //RaiseGMessage("G0HQ,2,4,0,2,4,5,6");
            //RaiseGMessage("G0HQ,2,1,0,10,34,50,40");
            //RaiseGMessage("G0HQ,2,2,0,42,6,3,49");
            //RaiseGMessage("G0HQ,2,3,0,23,41,20");
            //RaiseGMessage("G0HQ,2,4,0,16,19,43");
            //RaiseGMessage("G0HQ,2,5,0,37,5,27");
            //RaiseGMessage("G0HQ,2,6,0,32,46,53");
            //Board.EvePiles.PushBack(3);
            //ushort[] parts = Board.EvePiles.Dequeue(6);
            //Board.EvePiles.PushBack(39);
            //Board.EvePiles.PushBack(17);
            //Board.EvePiles.PushBack(18);
            //Board.EvePiles.PushBack(parts);
            //Board.RestNPCPiles.PushBack(1001);
            //Board.HeroPiles.PushBack(10403);
            //RaiseGMessage("G0HQ,2,3,0,50");
            // RaiseGMessage(new Artiad.EquipStandard()
            // {
            //     Who = 4,
            //     Source = 0,
            //     Coach = 4,
            //     SlotAssign = true,
            //     SingleCard = 49
            // }.ToMessage());
            // RaiseGMessage(new Artiad.EquipStandard()
            // {
            //     Who = 1,
            //     Source = 0,
            //     Coach = 1,
            //     SlotAssign = true,
            //     Cards = new ushort[] { 50, 109, 110, 111 }
            // }.ToMessage());
            //RaiseGMessage("G0ZB,4,0,51");
            //RaiseGMessage("G0HQ,2,1,0,1,50,49");
            //RaiseGMessage("G0HQ,2,2,0,44,51");
            //RaiseGMessage("G0HQ,2,1,0,16,43");
            //Board.MonPiles.Dequeue(29);
            //Board.MonPiles.PushBack(1009);
            //RaiseGMessage("G0ZB,6,0,47");
            //RaiseGMessage("G0ZB,2,0,55");
            //RaiseGMessage("G0ZB,1,0,49");
            //RaiseGMessage("G0ZB,1,0,52");
            //RaiseGMessage("G0ZB,3,0,48");
            //RaiseGMessage("G0ZB,3,0,53");
            //RaiseGMessage("G0ZB,5,0,49");
            //RaiseGMessage("G0ZB,1,0,74");
            //RaiseGMessage("G0ZB,4,0,52");
            //RaiseGMessage("G0ZB,5,0,54");
            //RaiseGMessage("G0ZB,2,0,49");
            //RaiseGMessage("G0ZB,3,0,52");
            //RaiseGMessage("G0ZB,2,0,55");
            //RaiseGMessage("G0ZB,2,0,51");
            //RaiseGMessage("G0HQ,2,1,0,49");
            //RaiseGMessage("G0HQ,2,3,0,19,73");
            //RaiseGMessage("G0HQ,2,4,0,47,48,52");
            //RaiseGMessage("G0ZB,1,0,49");
            //RaiseGMessage("G0ZB,3,0,73");
            //RaiseGMessage("G0ZB,4,0,47");
            //RaiseGMessage("G0ZB,4,0,48");
            //Board.Garden[1].Escue.Add(1001);
            //Board.Garden[3].Escue.Add(1002);
            //Board.Garden[4].Escue.Add(1003);
            //RaiseGMessage("G2IL,1,1001,3,1002,4,1003");
            //RaiseGMessage("G0ZB,4,0,52");
            //RaiseGMessage("G0HD,5,0,0,6");
            //RaiseGMessage("G0HD,5,0,0,17");
            //RaiseGMessage("G0HD,2,0,0,24");
            //RaiseGMessage("G0HD,1,0,0,50");
            //RaiseGMessage("G0HD,5,0,0,36");
            //RaiseGMessage("G0HD,6,0,0,16");
            //RaiseGMessage("G0HD,2,0,0,51");
            //RaiseGMessage("G0HD,2,0,0,33");
            //RaiseGMessage("G0HD,3,0,0,7");
            // RaiseGMessage("G0HD,3,0,0,49");
            //RaiseGMessage("G0HD,5,0,0,22");
            //RaiseGMessage("G0HD,4,0,0,19,1");
            //RaiseGMessage("G0HD,6,0,0,4");
            //RaiseGMessage("G0HD,1,0,0,45");
            //RaiseGMessage("G0HD,1,0,0,29");
            //RaiseGMessage("G0HD,1,0,0,41");
            //RaiseGMessage("G0HD,5,0,0,68");
            //RaiseGMessage("G0HD,3,0,0,70");
            //RaiseGMessage("G0HD,6,0,0,19");
            //RaiseGMessage("G0HD,1,0,0,5");
            //RaiseGMessage("G0IJ,1,1,2,E1,E2");
            //RaiseGMessage("G0HD,1,0,0,69");
            //RaiseGMessage("G0HD,4,0,0,74");
            //RaiseGMessage("G0HD,2,0,0,35");
            //RaiseGMessage("G0HQ,1,0,1,16,5,44,50");
            //RaiseGMessage("G0HQ,4,0,1,43");
            //RaiseGMessage("G0OH,1,0,0,5,2,0,0,4,3,0,0,5,4,0,0,5,5,0,0,5,6,0,0,5");
            //RaiseGMessage("G0OH,2,0,0,2,3,0,0,3,4,0,0,5,6,0,0,2");
            //RaiseGMessage("G0OH,1,0,4,12,2,0,4,12");
            // RaiseGMessage(Artiad.Harm.ToMessage(new ushort[] { 1, 3, 5 }
            //     .Select(p => new Artiad.Harm(p, 1, FiveElement.YINN, 5, 0))));
            //RaiseGMessage(Artiad.Harm.ToMessage(new ushort[] { 1, 4, 5, 6 }
            //    .Select(p => new Artiad.Harm(p, 1, FiveElement.YIN, 6, 0))));
            //RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(1, 1, FiveElement.AQUA, 8, 0)));
            //RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(5, 5, FiveElement.AQUA, 4, 0)));
            //RaiseGMessage(Artiad.Harm.ToMessage(new Artiad.Harm(2, 2, FiveElement.AQUA, 5, 0)));
            //Board.Garden[3].Escue.Add(1112);
            //RaiseGMessage("G2IL,3,1112");
            //RaiseGMessage("G0IF,6,3,4,6");
            //RaiseGMessage("G0IF,6,1,2,3,4");
            //RaiseGMessage("G0IF,4,1,2,3,4");
            //RaiseGMessage("G0IF,1,2");
            //RaiseGMessage("G0IF,4,4");
            //foreach (Player player in Board.Garden.Values)
            //   RaiseGMessage("G0HQ,2," + player.Uid + ",1,3");
            //RaiseGMessage("G0HQ,2,4,1,8");
            //RaiseGMessage("G0HQ,2,6,1,2");
            //RaiseGMessage("G0HQ,2,1,1,1");
            //RaiseGMessage("G0HQ,2,2,1,3");
            //RaiseGMessage("G0HQ,2,3,1,4");
            //RaiseGMessage("G0HQ,2,4,1,6");
            //RaiseGMessage("G0HQ,2,5,1,3");
            //RaiseGMessage("G0HQ,2,6,1,3");
            //while (true)
            //{
            //    string inp = AsyncInput((ushort)6, "/D10~16", "XID", "0");
            //    if (inp.StartsWith("/"))
            //        break;
            //    RaiseGMessage("G0DH,6,0," + inp);
            //}
        }
    }
}