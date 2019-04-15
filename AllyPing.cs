using System;
using System.Reflection;
using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace AllyPing {

    [BepInPlugin("dev.morris1927.ror2.allyping", "AllyPing", "1.0.0")]
    public class AllyPing : BaseUnityPlugin
    {
        public static FieldInfo currentEnemy = typeof(BaseAI).GetField("currentEnemy", BindingFlags.NonPublic | BindingFlags.Instance);
        public static FieldInfo pingerController = typeof(PlayerCharacterMasterController).GetField("pingerController", BindingFlags.NonPublic | BindingFlags.Instance);


        private void Awake() {

            IL.RoR2.CharacterBody.HandleConstructTurret += (il) => {
                var c = new ILCursor(il);

                c.GotoNext(x => x.MatchStloc(4));
                c.Emit(OpCodes.Ldloc_2);
                c.EmitDelegate<Func<CharacterMaster, CharacterMaster, CharacterMaster>>((turret, master) => {
                    turret.gameObject.AddComponent<AIOwnership>().ownerMaster = master;
                    
                    return turret;
                });

            };


            IL.RoR2.CharacterAI.BaseAI.FixedUpdate += (il2) => {
                var c2 = new ILCursor(il2);
                c2.Emit(OpCodes.Ldarg_0);

                c2.EmitDelegate<Action<BaseAI>>((ai) => {
                    if (ai.leader.characterBody != null) {
                        BaseAI.Target leader = ai.leader;


                        if (leader.characterBody.isPlayerControlled) {
                            PingerController p = null; //leader.characterBody.master.GetComponent<PingerController>(); 
                            foreach (PlayerCharacterMasterController item in PlayerCharacterMasterController.instances) {
                                if (item.master.GetBody().netId == leader.characterBody.netId) {
                                    p = (PingerController) pingerController.GetValue(item);
                                }
                            }
                            if (p != null) {
                                if (p.currentPing.targetGameObject != null) {
                                    ((BaseAI.Target)currentEnemy.GetValue(ai)).gameObject = p.currentPing.targetGameObject;
                                }
                            }
                        }
                    }
                });
           
            };

        }
    }
}
