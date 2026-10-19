using System;
using UnityEditor;
using UnityEngine;

namespace IdleGarden.Editor
{
    public static class GardenFeatureVerification
    {
        public static void Check(bool pass,string message) { if(!pass) throw new Exception(message); }
        [MenuItem("Tools/Idle Garden/Verify F02 Pot Themes and Upgrades")]
        public static void Pots()
        {
            var profile=AssetDatabase.LoadAssetAtPath<GardenProfile>(GardenPrototypeBuilder.ProfilePath);
            var config=profile.progression;
            Check(config.Validate(out _),"Invalid content catalog");
            var funds=new GardenSaveData {gold=1000000};
            var p=new GardenProgression(config,funds); var sim=new GardenSimulation(profile,p);
            var fire=config.FindCrop("fire-pepper");
            Check(!sim.SelectCrop(0,fire),"Locked crop selected");
            double gold=p.Gold; Check(p.UnlockTheme(fire.theme),"Theme unlock failed");
            Check(!p.UnlockTheme(fire.theme)&&p.Gold==gold-fire.theme.unlockGold&&p.Chests==1,"Theme charged/rewarded twice");
            var original=sim.Plots[0].Crop; var other=sim.Plots[1].SelectedCrop;
            sim.Plots[0].Planted=true; sim.Plots[0].Growth=.5f;
            Check(sim.SelectCrop(0,fire)&&sim.Plots[0].Crop==original&&sim.Plots[0].SelectedCrop==fire&&sim.Plots[0].Growth==.5f,"Mid-vue selection destroyed crop");
            Check(sim.Plots[1].SelectedCrop==other,"Selection leaked to another pot");
            sim.Plots[0].Growth=1;
            for(int i=0;i<500 && sim.Plots[0].Crop!=fire;i++) sim.Tick(.05f);
            Check(sim.Plots[0].Crop==fire,"Pending crop did not apply after harvest");
            var data=p.Capture(); sim.CaptureInto(data); var restored=new GardenSimulation(profile,new GardenProgression(config,data)); restored.Restore(data);
            Check(restored.Plots[0].SelectedCrop==fire,"Pot selection lost on capture/restore");
            float baseSpeed=profile.worker.moveSpeed;
            p.AutoSell=false;
            foreach(var crop in config.crops) p.AddHarvest(crop,10000);
            for(int i=0;i<config.upgrades.Length;i++)
            {
                double before=p.Gold, cost=p.GoldCost(i); long material=p.Count(config.upgrades[i].material), required=p.MaterialCost(i);
                Check(p.TryBuy(i)&&p.Level(i)==1&&p.Gold==before-cost,"Upgrade purchase mismatch");
                Check(p.Count(config.upgrades[i].material)==material-required,"Material cost mismatch");
                Check(p.Multiplier(config.upgrades[i].stat,config.upgrades[i].targetCrop)>1,"Upgrade effect missing");
            }
            Check(profile.worker.moveSpeed==baseSpeed,"Base worker data mutated");
            var poor=new GardenProgression(config); Check(!poor.TryBuy(-1)&&!poor.TryBuy(999)&&!poor.TryBuy(0),"Invalid/unaffordable upgrade bought");
            Debug.Log("PASS F02: independent pot selection, deferred planting, unlock once, upgrade costs/effects, immutable base data, selection capture.");
        }
    }
}
