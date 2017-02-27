using System;
using System.Collections.Generic;
using System.Linq;
using PSD.Base;
using PSD.Base.Card;
using Algo = PSD.Base.Utils.Algo;

namespace PSD.PSDGamepkg.Artiad
{
    public class HarvestPet
    {
        // KOKAN:return the old one; ACTIVE:farmer make chose; PASSIVE: erase the old one directly
        public enum Treaty { NL = 0, KOKAN = 1, ACTIVE = 2, PASSIVE = 3 };
        // the one to harvest the pet
        public ushort Farmer { set; get; }
        // the one to provide the pet 
        public ushort Farmland { set; get; }
        // pets
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        // whether gain from battle, used only for trigger
        public bool Trophy { set; get; }
        // whether put the card back to whether it froms if not pick up, only valid in ACTIVE
        public bool Reposit { set; get; }
        // whether plow the farmland: trigger pet lose or put into dices action, only valid in ACTIVE
        public bool Plow { set; get; }
        // Treaty
        public Treaty TreatyAct { set; get; }

        public HarvestPet()
        {
            Trophy = false; TreatyAct = Treaty.ACTIVE;
            Farmland = 0; Reposit = false; Plow = true;
        }
        public string ToMessage()
        {
            int mask = (int)TreatyAct;
            if (Trophy) { mask |= 0x4; }
            if (Reposit) { mask |= 0x8; }
            if (Plow) { mask |= 0x10; }
            return "G0HC,0," + Farmer + "," + Farmland + "," + mask + "," + string.Join(",", Pets);
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            new HarvestPetSemaphore()
            {
                Farmer = Farmer,
                Pets = Pets
            }.Telegraph(WI.BCast);
            // Item,         HL, ON
            // Farmland = 0  Y   -
            //          / 0  -   -
            // Reposite = T  N   N
            //          = F  -   -
            // Plow     = T  Y   Y
            //          = F  N   Y
            Player player = XI.Board.Garden[Farmer];
            int fivepc = FiveElementHelper.PropCount;
            List<ushort>[] cpets = new List<ushort>[fivepc];
            for (int i = 0; i < fivepc; ++i)
            {
                cpets[i] = new List<ushort>();
                if (player.Pets[i] != 0)
                    cpets[i].Add(player.Pets[i]);
            }
            foreach (ushort petUt in Pets)
            {
                Monster pet = XI.LibTuple.ML.Decode(petUt);
                int pe = pet.Element.Elem2Index();
                if (!cpets[pe].Contains(petUt))
                    cpets[pe].Add(petUt);
            }
            List<ushort> result = new List<ushort>();
            List<ushort> giveBack = new List<ushort>();
            bool needHL = Farmland != 0 && Plow;
            for (int i = 0; i < fivepc; ++i)
            {
                if (cpets[i].Count == 0)
                    continue;
                else if (cpets[i].Count == 1)
                {
                    ushort pt = cpets[i].First();
                    if (Pets.Contains(pt))
                    {
                        if (needHL)
                        {
                            XI.RaiseGMessage(new Artiad.LosePet()
                            {
                                Owner = Farmland,
                                SinglePet = pt,
                                Stepper = Farmer,
                                Recycle = false
                            }.ToMessage());
                        }
                        result.Add(pt);
                    }
                    continue;
                }
                Treaty treaty = TreatyAct;
                if (cpets[i].Count > 2)
                    treaty = Treaty.ACTIVE; // more than two selection
                if (Farmland == 0 && treaty == Artiad.HarvestPet.Treaty.KOKAN)
                    treaty = Treaty.ACTIVE;
                ushort myPt = player.Pets[i];
                if (treaty == Treaty.KOKAN) // KOKAN always recycle
                {
                    ushort ayPt = SinglePet;
                    if (needHL)
                    {
                        XI.RaiseGMessage(new Artiad.LosePet()
                        {
                            Owner = Farmland,
                            SinglePet = ayPt,
                            Stepper = Farmer,
                            Recycle = false
                        }.ToMessage());
                    }
                    result.Add(ayPt);
                    XI.RaiseGMessage(new Artiad.LosePet()
                    {
                        Owner = Farmer,
                        SinglePet = myPt,
                        Stepper = Farmland,
                        Recycle = false
                    }.ToMessage());
                    giveBack.Add(myPt);
                }
                else if (treaty == Artiad.HarvestPet.Treaty.PASSIVE)
                {
                    ushort ayPt = SinglePet;
                    XI.RaiseGMessage(new Artiad.LosePet()
                    {
                        Owner = Farmer,
                        SinglePet = myPt
                    }.ToMessage());
                    if (needHL)
                    {
                        XI.RaiseGMessage(new Artiad.LosePet()
                        {
                            Owner = Farmland,
                            SinglePet = ayPt,
                            Stepper = Farmer,
                            Recycle = false
                        }.ToMessage());
                    }
                    result.Add(ayPt);
                }
                else // ACTIVE
                {
                    List<ushort> others = cpets[i].ToList(); others.Remove(myPt);
                    string mai = "#保留的,M1(p" + string.Join("p", cpets[i]) + ")";
                    ushort sel = ushort.Parse(XI.AsyncInput(Farmer, mai, "G0HC", "0"));
                    if (sel == myPt) // Keep the old one
                    {
                        if (!Reposit) // if reposit, then leave it where it was
                        {
                            if (needHL)
                            {
                                XI.RaiseGMessage(new Artiad.LosePet()
                                {
                                    Owner = Farmland,
                                    Pets = others.ToArray(),
                                    Recycle = false
                                }.ToMessage());
                            }
                            XI.RaiseGMessage(new Artiad.Abandon()
                            {
                                Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                                Genre = Card.Genre.NMB,
                                SingleUnit = new Artiad.CustomsUnit()
                                {
                                    Source = Farmland,
                                    Cards = others.ToArray()
                                }
                            }.ToMessage());
                        }
                    }
                    else
                    {
                        others.Remove(sel);
                        // lose old myself
                        XI.RaiseGMessage(new Artiad.LosePet()
                        {
                            Owner = Farmer,
                            SinglePet = myPt
                        }.ToMessage());
                        // lose the selection
                        if (needHL)
                        {
                            XI.RaiseGMessage(new Artiad.LosePet()
                            {
                                Owner = Farmland,
                                SinglePet = sel,
                                Stepper = Farmer,
                                Recycle = false
                            }.ToMessage());
                        }
                        result.Add(sel);
                        // remove if reposit is not set, otherwise put it back
                        if (others.Count > 0 && !Reposit)
                        {
                            if (needHL)
                            {
                                XI.RaiseGMessage(new Artiad.LosePet()
                                {
                                    Owner = Farmland,
                                    Pets = others.ToArray(),
                                    Recycle = false
                                }.ToMessage());
                            }
                            XI.RaiseGMessage(new Artiad.Abandon()
                            {
                                Zone = Artiad.CustomsHelper.ZoneType.PLAYER,
                                Genre = Card.Genre.NMB,
                                SingleUnit = new Artiad.CustomsUnit()
                                {
                                    Source = Farmland,
                                    Cards = others.ToArray()
                                }
                            }.ToMessage());
                        }
                    }
                }
            }
            if (result.Count > 0)
            {
                XI.RaiseGMessage(new Artiad.ObtainPet()
                {
                    Farmer = Farmer,
                    Farmland = Farmland,
                    Trophy = Trophy,
                    Pets = result.ToArray()
                }.ToMessage());
            }
            if (giveBack.Count > 0)
            {
                XI.RaiseGMessage(new Artiad.ObtainPet()
                {
                    Farmer = Farmland,
                    Farmland = Farmer,
                    Trophy = Trophy,
                    Pets = giveBack.ToArray()
                }.ToMessage());
            }
        }
        public static HarvestPet Parse(string line)
        {
            ushort[] g0hc = line.Substring("G0HC,0,".Length).Split(',').Select(p => ushort.Parse(p)).ToArray();
            int mask = (int)g0hc[2];
            return new HarvestPet()
            {
                Farmer = g0hc[0],
                Farmland = g0hc[1],
                Pets = Algo.TakeRange(g0hc, 3, g0hc.Length),
                Trophy = (mask & 0x4) != 0,
                Reposit = (mask & 0x8) != 0,
                Plow = (mask & 0x10) != 0,
                TreatyAct = (mask & 0x3) == 1 ? Treaty.KOKAN : ((mask & 0x3) == 2 ? Treaty.ACTIVE : Treaty.PASSIVE)
            };
        }
        public string ToPrompt(XI XI, string language)
        {
            if (language == "ZH")
                return XI.DisplayPlayer(Farmer) + "将获得" + XI.DisplayMonster(Pets);
            return "";
        }
    }
    // trade the pet between two players
    public class TradePet
    {
        public ushort A { set; get; }
        public ushort[] AGoods { set; get; }
        public ushort ASinglePet { set { AGoods = new ushort[] { value }; } }
        public ushort B { set; get; }
        public ushort[] BGoods { set; get; }
        public ushort BSinglePet { set { BGoods = new ushort[] { value }; } }
        public string ToMessage()
        {
            return "G0HC,1," + A + "," + Algo.ListToString(AGoods.ToList()) +
                "," + B + "," + Algo.ListToString(BGoods.ToList());
        }
        public static TradePet Parse(string line)
        {
            ushort[] g0hc = line.Substring("G0HC,1,".Length).Split(',').Select(p => ushort.Parse(p)).ToArray();
            int index = 0;
            return new TradePet()
            {
                A = g0hc[index],
                AGoods = Algo.TakeArrayWithSize(g0hc, index + 1, out index),
                B = g0hc[index],
                BGoods = Algo.TakeArrayWithSize(g0hc, index + 1, out index)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            if (AGoods.Length > 0)
                XI.RaiseGMessage(new Artiad.LosePet()
                {
                    Owner = A,
                    Pets = AGoods,
                    Stepper = B,
                    Recycle = false
                }.ToMessage());
            if (BGoods.Length > 0)
                XI.RaiseGMessage(new Artiad.LosePet()
                {
                    Owner = B,
                    Pets = BGoods,
                    Stepper = A,
                    Recycle = false
                }.ToMessage());
            if (BGoods.Length > 0)
                XI.RaiseGMessage(new Artiad.HarvestPet()
                {
                    Farmer = A,
                    Farmland = B,
                    Pets = BGoods.ToArray(),
                    Reposit = false,
                    Plow = false
                }.ToMessage());
            if (AGoods.Length > 0)
                XI.RaiseGMessage(new Artiad.HarvestPet()
                {
                    Farmer = B,
                    Farmland = A,
                    Pets = AGoods.ToArray(),
                    Reposit = false,
                    Plow = false
                }.ToMessage());
        }
    }
    // actually obtain the pet
    public class ObtainPet
    {
        // the one to harvest the pet
        public ushort Farmer { set; get; }
        // the one to provide the pet 
        public ushort Farmland { set; get; }
        // whether gain from battle
        public bool Trophy { set; get; }
        // pets
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        public ObtainPet() { Farmland = 0; Trophy = false; }
        public string ToMessage()
        {
            return "G0HD," + Farmer + "," + Farmland + "," +
                (Trophy ? 1 : 0) + "," + string.Join(",", Pets);
        }
        public static ObtainPet Parse(string line)
        {
            ushort[] g0hc = line.Substring("G0HD,".Length).Split(',').Select(p => ushort.Parse(p)).ToArray();
            return new ObtainPet()
            {
                Farmer = g0hc[0],
                Farmland = g0hc[1],
                Trophy = (g0hc[2] == 1),
                Pets = Algo.TakeRange(g0hc, 3, g0hc.Length)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player player = XI.Board.Garden[Farmer];
            foreach (ushort which in Pets)
            {
                Monster pet = XI.LibTuple.ML.Decode(which);
                player.Pets[pet.Element.Elem2Index()] = which;
                XI.RaiseGMessage("G0WB," + which);
            }
            if (!player.PetDisabled)
            {
                new JoinPetEffects()
                {
                    SingleUnit = new PetEffectUnit()
                    {
                        Owner = Farmer,
                        Pets = Pets.Where(p => p != 0 &&
                            XI.LibTuple.ML.Decode(p).Seals.Count == 0).ToArray(),
                        Reload = (Farmland == 0 || XI.Board.Garden[Farmland].Team != player.Team) ?
                            PetEffectUnit.ReloadType.NEW : PetEffectUnit.ReloadType.BORROW
                    }
                }.Hotel(XI);
            }
            new ObtainPetSemaphore() { Farmer = Farmer, Farmland = Farmland, Pets = Pets }.Telegraph(WI.BCast);
            XI.RaiseGMessage("G2WK," + string.Join(",",
                XI.CalculatePetsScore().Select(p => p.Key + "," + p.Value)));
        }
    }
    // lose pet
    public class LosePet
    {
        public ushort Owner { set; get; }
        // pets
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet { set { Pets = new ushort[] { value }; } }
        // possible next owner, or pile
        public ushort Stepper { set; get; }
        // whether need to put it into piles
        public bool Recycle { set; get; }
        public LosePet() { Recycle = true; Stepper = 0; }
        public string ToMessage()
        {
            return "G0HL," + (Recycle ? 1 : 0) + "," + Owner + "," + Stepper + "," + string.Join(",", Pets);
        }
        public static LosePet Parse(string line)
        {
            ushort[] g0hl = line.Substring("G0HL,".Length).Split(',').Select(p => ushort.Parse(p)).ToArray();
            return new LosePet()
            {
                Owner = g0hl[1],
                Stepper = g0hl[2],
                Pets = Algo.TakeRange(g0hl, 3, g0hl.Length),
                Recycle = (g0hl[0] == 1)
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            Player player = XI.Board.Garden[Owner];
            ushort[] pets = Pets.Where(p => player.Pets[
                XI.LibTuple.ML.Decode(p).Element.Elem2Index()] == p).ToArray();
            if (pets.Length > 0)
            {
                if (!player.PetDisabled)
                {
                    new CollapsePetEffects()
                    {
                        SingleUnit = new PetEffectUnit()
                        {
                            Owner = Owner,
                            Pets = Pets.Where(p => p != 0 &&
                                XI.LibTuple.ML.Decode(p).Seals.Count == 0).ToArray(),
                            Reload = (Stepper == 0 || XI.Board.Garden[Stepper].Team != player.Team) ?
                                PetEffectUnit.ReloadType.NEW : PetEffectUnit.ReloadType.BORROW
                        }
                    }.Hotel(XI);
                }
                foreach (ushort pet in Pets)
                {
                    player.Pets[XI.LibTuple.ML.Decode(pet).Element.Elem2Index()] = 0;
                    XI.RaiseGMessage("G0WB," + pet);
                }
                new LosePetSemaphore() { Owner = Owner, Pets = pets }.Telegraph(WI.BCast);
                // TODO: stepper to indicate whether fly to dices or not
                XI.RaiseGMessage("G2WK," + string.Join(",",
                    XI.CalculatePetsScore().Select(p => p.Key + "," + p.Value)));
                if (Recycle)
                    XI.RaiseGMessage(new Abandon()
                    {
                        Zone = CustomsHelper.ZoneType.PLAYER,
                        Genre = Card.Genre.NMB,
                        SingleUnit = new CustomsUnit() { Source = Owner, Cards = pets.ToArray() }
                    }.ToMessage());
            }
        }
    }
    // Pet's effect take action, specially to handle with buffer Incr
    public class JoinPetEffects : NGT
    {
        public List<PetEffectUnit> List { set; get; }
        public PetEffectUnit SingleUnit
        {
            set { List = new List<PetEffectUnit>() { value }; }
        }

        public override bool Legal()
        {
            return List.Count > 0 && List.Any(peu => peu.Pets.Length > 0);
        }
        public override string ToMessage()
        {
            return "G0IC," + string.Join(",", List.Select(p => p.ToRawMessage()));
        }
        public static JoinPetEffects Parse(string line)
        {
            return new JoinPetEffects() { List = PetEffectUnit.ParseFromLine(line) };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            List.ForEach(jpes =>
            {
                Player player = XI.Board.Garden[jpes.Owner];
                foreach (ushort pt in jpes.Pets)
                {
                    Monster pet = XI.LibTuple.ML.Decode(pt);
                    if (jpes.Reload == PetEffectUnit.ReloadType.NEW)
                        pet.TeamBursted = false;
                    if (jpes.Reload != PetEffectUnit.ReloadType.ABLE)
                    {
                        pet.ResetROM();
                        XI.RaiseGMessage("G0WB," + pt);
                        // pet.SetIncrOption(player);
                    }
                    pet.IncrAction(player);
                }
                new JoinPetSemaphore() { Owner = jpes.Owner, Pets = jpes.Pets }.Telegraph(WI.BCast);
            });
        }
    }
    // Pet's effect lost, especially to handle with buffer Incr
    public class CollapsePetEffects : NGT
    {
        public List<PetEffectUnit> List { set; get; }
        public PetEffectUnit SingleUnit
        {
            set { List = new List<PetEffectUnit>() { value }; }
        }

        public override bool Legal()
        {
            return List.Count > 0 && List.Any(peu => peu.Pets.Length > 0);
        }
        public override string ToMessage()
        {
            return "G0OC," + string.Join(",", List.Select(p => p.ToRawMessage()));
        }
        public static CollapsePetEffects Parse(string line)
        {
            return new CollapsePetEffects() { List = PetEffectUnit.ParseFromLine(line) };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            List.ForEach(jpes =>
            {
                Player player = XI.Board.Garden[jpes.Owner];
                foreach (ushort pt in jpes.Pets)
                {
                    Monster pet = XI.LibTuple.ML.Decode(pt);
                    pet.DecrAction(player);
                    if (jpes.Reload != PetEffectUnit.ReloadType.ABLE)
                        pet.ResetROM();
                    if (jpes.Reload == PetEffectUnit.ReloadType.NEW)
                        pet.TeamBursted = false;
                    XI.RaiseGMessage("G0WB," + pt);
                }
                new CollapsePetSemaphore() { Owner = jpes.Owner, Pets = jpes.Pets }.Telegraph(WI.BCast);
            });
        }
    }
    #region Pet Effect Unit
    public class PetEffectUnit
    {
        // NEW=Reset all;BORROW=Not Reset Team Tage;ABLE=Enable
        public enum ReloadType { NEW, BORROW, ABLE };
        public ReloadType Reload { set; get; }
        public ushort Owner { set; get; }
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        internal string ToRawMessage()
        {
            int typeToInt = Reload == ReloadType.ABLE ? 2 : (Reload == ReloadType.BORROW ? 1 : 0);
            return typeToInt + "," + Owner + "," + Algo.ListToString(Pets);
        }
        internal static List<PetEffectUnit> ParseFromLine(string line)
        {
            List<PetEffectUnit> jpes = new List<PetEffectUnit>();
            string[] peui = line.Split(',');
            for (int idx = 1; idx < peui.Length;)
            {
                int typeToInt = int.Parse(peui[idx++]);
                ushort owner = ushort.Parse(peui[idx++]);
                ushort[] pets = Algo.TakeArrayWithSize(peui, idx, out idx);
                jpes.Add(new PetEffectUnit()
                {
                    Reload = typeToInt == 3 ? PetEffectUnit.ReloadType.ABLE : (typeToInt == 2 ?
                        PetEffectUnit.ReloadType.BORROW : PetEffectUnit.ReloadType.NEW),
                    Owner = owner,
                    Pets = pets
                });
            }
            return jpes;
        }
    }
    #endregion Pet Effect Unit

    // enable a player's all pet effect
    public class EnablePlayerPetEffect
    {
        public ushort[] Who { set; get; }
        public ushort SingleWho
        {
            set { Who = new ushort[] { value }; }
            get { return (Who != null && Who.Length == 1) ? Who[0] : (ushort)0; }
        }
        public string ToMessage() { return "G0IE,0," + string.Join(",", Who); }
        public static EnablePlayerPetEffect Parse(string line)
        {
            string[] g0ie = line.Split(',');
            return new EnablePlayerPetEffect()
            {
                Who = Algo.TakeRange(g0ie, 2, g0ie.Length).Select(p => ushort.Parse(p)).ToArray()
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            List<Player> pys = Who.Select(p => XI.Board.Garden[p]).Where(p => p.PetDisabled).ToList();
            if (pys.Count > 0)
            {
                List<Player> pyis = pys.Where(p => p.Pets.Any(q => q != 0 &&
                    !XI.LibTuple.ML.Decode(q).Seals.Any())).ToList();
                XI.RaiseGMessage(new Artiad.JoinPetEffects()
                {
                    List = pyis.Select(p => new Artiad.PetEffectUnit()
                    {
                        Owner = p.Uid,
                        Pets = p.Pets.Where(q => q != 0 && !XI.LibTuple.ML.Decode(q).Seals.Any()).ToArray(),
                        Reload = Artiad.PetEffectUnit.ReloadType.ABLE
                    }).ToList()
                }.ToMessage());
                pys.ForEach(p => p.PetDisabled = false);
                new EnablePetEffectSemaphore()
                {
                    IsPlayer = true,
                    Targets = pys.Select(p => p.Uid).ToArray()
                }.Telegraph(WI.BCast);
            }
        }
    }
    // enable pet effect of a pet, might be shielded by player-level disable
    public class EnableItPetEffect : NGT
    {
        public ushort[] Its { set; get; }
        public ushort SingleIt
        {
            set { Its = new ushort[] { value }; }
            get { return (Its != null && Its.Length == 1) ? Its[0] : (ushort)0; }
        }
        public string Reason { set; get; }

        public override bool Legal() { return Its.Length > 0 && !string.IsNullOrEmpty(Reason); }
        public override string ToMessage() { return "G0IE,1," + Reason + "," + string.Join(",", Its); }
        public static EnableItPetEffect Parse(string line)
        {
            string[] g0ie = line.Split(',');
            return new EnableItPetEffect()
            {
                Reason = g0ie[2],
                Its = Algo.TakeRange(g0ie, 3, g0ie.Length).Select(p => ushort.Parse(p)).ToArray()
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            // 1. find those did matters
            List<ushort> pets = Its.Where(p => p != 0 && Artiad.ContentRule.GetPetOwnership(p, XI) != 0 &&
                !XI.Board.Garden[Artiad.ContentRule.GetPetOwnership(p, XI)].PetDisabled &&
                XI.LibTuple.ML.Decode(p).Seals.Count == 1 && XI.LibTuple.ML.Decode(p).Seals.Contains(Reason)).ToList();
            // 2. erase the reason from valid pets
            Its.Where(p => p != 0).Select(p => XI.LibTuple.ML.Decode(p)).ToList().ForEach(p => p.Seals.Remove(Reason));
            // 3. change whether a pet is totally freed
            if (pets.Count > 0)
            {
                List<Artiad.PetEffectUnit> peuList = Artiad.ContentRule.GetPetOwnershipTable(
                    pets, XI).Select(p => new Artiad.PetEffectUnit()
                    {
                        Owner = p.Key,
                        Pets = p.ToArray(),
                        Reload = Artiad.PetEffectUnit.ReloadType.ABLE
                    }).ToList();
                XI.RaiseGMessage(new Artiad.JoinPetEffects() { List = peuList }.ToMessage());
                new EnablePetEffectSemaphore() { IsPlayer = false, Targets = pets.ToArray() }.Telegraph(WI.BCast);
            }
        }
    }
    // disable a player's all pet effect
    public class DisablePlayerPetEffect
    {
        public ushort[] Who { set; get; }
        public ushort SingleWho
        {
            set { Who = new ushort[] { value }; }
            get { return (Who != null && Who.Length == 1) ? Who[0] : (ushort)0; }
        }
        public string ToMessage() { return "G0OE,0," + string.Join(",", Who); }
        public static DisablePlayerPetEffect Parse(string line)
        {
            string[] g0oe = line.Split(',');
            return new DisablePlayerPetEffect()
            {
                Who = Algo.TakeRange(g0oe, 2, g0oe.Length).Select(p => ushort.Parse(p)).ToArray()
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {
            List<Player> pys = Who.Select(p => XI.Board.Garden[p]).Where(p => !p.PetDisabled).ToList();
            if (pys.Count > 0)
            {
                List<Player> pyis = pys.Where(p => p.Pets.Any(q => q != 0 &&
                    !XI.LibTuple.ML.Decode(q).Seals.Any())).ToList();
                if (pyis.Count > 0)
                    XI.RaiseGMessage(new CollapsePetEffects()
                    {
                        List = pyis.Select(p => new PetEffectUnit()
                        {
                            Owner = p.Uid,
                            Pets = p.Pets.Where(q => q != 0 && !XI.LibTuple.ML.Decode(q).Seals.Any()).ToArray(),
                            Reload = PetEffectUnit.ReloadType.ABLE
                        }).ToList()
                    }.ToMessage());
                pys.ForEach(p => p.PetDisabled = true);
                new DisablePetEffectSemaphore()
                {
                    IsPlayer = true,
                    Targets = pys.Select(p => p.Uid).ToArray()
                }.Telegraph(WI.BCast);
            }
        }
    }
    // disable pet effect of a pet, might be shielded by player-level disable
    public class DisableItPetEffect : NGT
    {
        public ushort[] Its { set; get; }
        public ushort SingleIt
        {
            set { Its = new ushort[] { value }; }
            get { return (Its != null && Its.Length == 1) ? Its[0] : (ushort)0; }
        }
        public string Reason { set; get; }

        public override bool Legal() { return Its.Length > 0 && !string.IsNullOrEmpty(Reason); }
        public override string ToMessage() { return "G0OE,1," + Reason + "," + string.Join(",", Its); }
        public static DisableItPetEffect Parse(string line)
        {
            string[] g0oe = line.Split(',');
            return new DisableItPetEffect()
            {
                Reason = g0oe[2],
                Its = Algo.TakeRange(g0oe, 3, g0oe.Length).Select(p => ushort.Parse(p)).ToArray()
            };
        }
        public void Handle(XI XI, Base.VW.IWISV WI)
        {         
            // 1. find those did matters
            List<ushort> pets = Its.Where(p => p != 0 && Artiad.ContentRule.GetPetOwnership(p, XI) != 0 &&
                !XI.Board.Garden[Artiad.ContentRule.GetPetOwnership(p, XI)].PetDisabled &&
                XI.LibTuple.ML.Decode(p).Seals.Count == 0).ToList();
            // 2. erase the reason from valid pets
            Its.Where(p => p != 0).Select(p => XI.LibTuple.ML.Decode(p)).ToList().ForEach(p => p.Seals.Add(Reason));
            // 3. change whether a pet is totally freed
            if (pets.Count > 0)
            {
                List<Artiad.PetEffectUnit> peuList = Artiad.ContentRule.GetPetOwnershipTable(
                    pets, XI).Select(p => new Artiad.PetEffectUnit()
                    {
                        Owner = p.Key,
                        Pets = p.ToArray(),
                        Reload = Artiad.PetEffectUnit.ReloadType.ABLE
                    }).ToList();
                XI.RaiseGMessage(new CollapsePetEffects() { List = peuList }.ToMessage());
                new DisablePetEffectSemaphore() { IsPlayer = false, Targets = pets.ToArray() }.Telegraph(WI.BCast);
            }
        }
    }

    public class HarvestPetSemaphore
    {
        public ushort Farmer { set; get; }
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        public void Telegraph(Action<string> send)
        {
            send("E0HC," + Farmer + "," + string.Join(",", Pets));
        }
    }

    public class ObtainPetSemaphore
    {
        public ushort Farmer { set; get; }
        public ushort Farmland { set; get; }
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet
        {
            set { Pets = new ushort[] { value }; }
            get { return (Pets != null && Pets.Length == 1) ? Pets[0] : (ushort)0; }
        }
        public void Telegraph(Action<string> send)
        {
            send("E0HD," + Farmer + "," + Farmland + "," + string.Join(",", Pets));
        }
    }

    public class LosePetSemaphore
    {
        public ushort Owner { set; get; }
        public ushort[] Pets { set; get; }
        // single pet case
        public ushort SinglePet { set { Pets = new ushort[] { value }; } }
        public void Telegraph(Action<string> send)
        {
            send("E0HL," + Owner + "," + string.Join(",", Pets));
        }
    }

    public class JoinPetSemaphore
    {
        public ushort Owner { set; get; }
        public ushort[] Pets { set; get; }
        public void Telegraph(Action<string> send)
        {
            send("E0IC," + Owner + "," + string.Join(",", Pets));
        }
    }
    public class CollapsePetSemaphore
    {
        public ushort Owner { set; get; }
        public ushort[] Pets { set; get; }
        public void Telegraph(Action<string> send)
        {
            send("E0OC," + Owner + "," + string.Join(",", Pets));
        }
    }
    public class EnablePetEffectSemaphore
    {
        public bool IsPlayer { set; get; }
        public ushort[] Targets { set; get; }
        public void Telegraph(Action<string> send)
        {
            send("E0IE," + (IsPlayer ? 0 : 1) + "," + string.Join(",", Targets));
        }
    }
    public class DisablePetEffectSemaphore
    {
        public bool IsPlayer { set; get; }
        public ushort[] Targets { set; get; }
        public void Telegraph(Action<string> send)
        {
            send("E0OE," + (IsPlayer ? 0 : 1) + "," + string.Join(",", Targets));
        }
    }

    public static class KittyHelper
    {
        public static bool IsHarvest(string line) { return line.StartsWith("G0HC,0"); }
        public static bool IsTrade(string line) { return line.StartsWith("G0HC,1"); }

        public static bool IsEnablePlayerPetEffect(string line) { return line.StartsWith("G0IE,0"); }
        public static bool IsEnableItPetEffect(string line) { return line.StartsWith("G0IE,1"); }

        public static bool IsDisablePlayerPetEffect(string line) { return line.StartsWith("G0OE,0"); }
        public static bool IsDisableItPetEffect(string line) { return line.StartsWith("G0OE,1"); }
    }
}