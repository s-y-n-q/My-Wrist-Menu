using ExitGames.Client.Photon;
using GorillaGameModes;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WristMenu.Mods
{
    public class Gamemode
    {
        // alot of these by kman off his menu thanks kman :3

        public static PhotonView GameModeView
        {
            get
            {
                var go = GameObject.Find("Player Objects/RigCache/Network Parent/GameMode(Clone)");
                if (go)
                    if (go.GetComponent<PhotonView>())
                        return go.GetComponent<PhotonView>();

                return null;
            }
        }

        public static void Tag(VRRig rig)
        {
            if (GorillaGameManager.instance is GorillaTagManager gtm)
            {
                if (!gtm.currentInfected.Contains(rig.Creator))
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (gtm.isCurrentlyTag) gtm.ChangeCurrentIt(rig.Creator);
                        else gtm.currentInfected.Add(rig.Creator);
                    }
                    else
                    {
                        UpdateRigPositionAndReport(rig, () => GameMode.ReportTag(rig.Creator));
                    }
                }
            }
            else if (GorillaGameManager.instance is GorillaHuntManager ghm)
            {
                if (!ghm.currentHunted.Contains(rig.Creator))
                {
                    if (PhotonNetwork.IsMasterClient) ghm.currentHunted.Add(rig.Creator);
                    else UpdateRigPositionAndReport(rig, () => GameMode.ReportTag(rig.Creator));
                }
            }
            else if (GorillaGameManager.instance is GorillaPaintbrawlManager gpm)
            {
                if (gpm.playerLives[rig.Creator.ActorNumber] > 0)
                {
                    if (PhotonNetwork.IsMasterClient) gpm.playerLives[rig.Creator.ActorNumber] = 0;
                    else
                    {
                        UpdateRigPositionAndReport(rig, () =>
                        {
                            GameModeView.SendUnlimmitedRPC("RPC_ReportSlingshotHit", RpcTarget.MasterClient, new object[]
                            {
                                rig.Creator,
                                rig.transform.position,
                                UnityEngine.Random.Range(0, 2000)
                            });
                        });
                    }
                }
            }
        }

        private static void UpdateRigPositionAndReport(VRRig rig, Action reportAction)
        {
            var runViewUpdate = typeof(PhotonNetwork).GetMethod("RunViewUpdate", BindingFlags.Static | BindingFlags.NonPublic);
            runViewUpdate.Invoke(null, Array.Empty<object>());

            var offlineRig = GorillaTagger.Instance.offlineVRRig;
            offlineRig.transform.position = offlineRig.rightHand.rigTarget.position = offlineRig.leftHand.rigTarget.position = rig.transform.position;

            runViewUpdate.Invoke(null, Array.Empty<object>());
            reportAction();
        }

       /* public static void SerilizeOneView(PhotonView pv)
        {
            try
            {
                mixedModeIsReliable_cache ??= typeof(PhotonView).GetField("mixedModeIsReliable", BindingFlags.Instance | BindingFlags.NonPublic);
                onSerilizeWrite_cache ??= typeof(PhotonNetwork).GetMethod("OnSerializeWrite", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(PhotonView) }, null);

                if (onSerilizeWrite_cache == null || mixedModeIsReliable_cache == null)
                {
                    Console.WriteLine("Could not serilize " + pv.name);
                    return;
                }

                var list = (List<object>)onSerilizeWrite_cache.Invoke(null, new object[] { pv });
                bool shouldBeReliable = pv.Synchronization == ViewSynchronization.ReliableDeltaCompressed || (bool)mixedModeIsReliable_cache.GetValue(pv);

                PhotonNetwork.NetworkingClient.OpRaiseEvent(
                    shouldBeReliable ? (byte)206 : (byte)201,
                    new object[] { PhotonNetwork.ServerTimestamp, null, list.Prepend(pv.ViewID).ToArray() },
                    new RaiseEventOptions { InterestGroup = pv.Group },
                    shouldBeReliable ? SendOptions.SendReliable : SendOptions.SendUnreliable
                );

                Console.WriteLine(list.ToJson());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToJson());
            }
        }*/

        /*public static void TagGun()
        {
            var data = GunLib.ShootLocked();
            if (data.lockedPlayer)
                Tag(data.lockedPlayer);
        }*/

        public static void TagAll()
        {
            foreach (var rig in GorillaParent.instance.vrrigs)
                Tag(rig);
        }

       /* public static void UntagGun()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var data = GunLib.ShootLocked();
            if (data.lockedPlayer)
            {
                if (GorillaGameManager.instance is GorillaTagManager gtm)
                {
                    if (gtm.isCurrentlyTag)
                    {
                        gtm.currentIt = null;
                        return;
                    }

                    if (gtm.currentInfected.Contains(data.lockedPlayer.Creator))
                        gtm.currentInfected.Remove(data.lockedPlayer.Creator);
                }
                else if (GorillaTagManager.instance is GorillaHuntManager ghm)
                {
                    if (ghm.currentHunted.Contains(data.lockedPlayer.Creator))
                        ghm.currentHunted.Remove(data.lockedPlayer.Creator);
                }
                else if (GorillaTagManager.instance is GorillaPaintbrawlManager gpm)
                {
                    if (gpm.playerLives[data.lockedPlayer.Creator.ActorNumber] < 3)
                        gpm.playerLives[data.lockedPlayer.Creator.ActorNumber] = 3;
                }
            }
        }*/

        public static void UntagSelf()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (GorillaGameManager.instance is GorillaTagManager gtm)
            {
                if (gtm.isCurrentlyTag)
                {
                    gtm.currentIt = null;
                    return;
                }

                if (gtm.currentInfected.Contains(PhotonNetwork.LocalPlayer))
                    gtm.currentInfected.Remove(PhotonNetwork.LocalPlayer);
            }
            else if (GorillaTagManager.instance is GorillaHuntManager ghm)
            {
                if (!ghm.currentHunted.Contains(PhotonNetwork.LocalPlayer))
                    ghm.currentHunted.Add(PhotonNetwork.LocalPlayer);
            }
            else if (GorillaTagManager.instance is GorillaPaintbrawlManager gpm)
            {
                if (gpm.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] < 3)
                    gpm.playerLives[PhotonNetwork.LocalPlayer.ActorNumber] = 3;
            }
        }

        public static void AntiTag()
        {
            UntagSelf();
        }
    }

    public static class PhotonManager
    {
        public static bool SendUnlimmitedRPC(this PhotonView photonView, string method, Player player,
            object[] parameters)
        {
            return SendUnlimmitedRPC(photonView, method, RpcTarget.AllBuffered, player, parameters);
        }

        public static bool SendUnlimmitedRPC(this PhotonView photonView, string method, RpcTarget player,
            object[] parameters)
        {
            return SendUnlimmitedRPC(photonView, method, player, null, parameters);
        }

        private static bool SendUnlimmitedRPC(PhotonView photonView, string method, RpcTarget target, Player player,
            object[] parameters)
        {
            if (photonView != null && parameters != null && !string.IsNullOrEmpty(method))
            {
                var rpcHash = new Hashtable
                {
                    { 0, photonView.ViewID },
                    { 2, PhotonNetwork.ServerTimestamp + -int.MaxValue },
                    { 3, method },
                    { 4, parameters }
                };

                if (photonView.Prefix > 0) rpcHash[1] = (short)photonView.Prefix;
                if (PhotonNetwork.PhotonServerSettings.RpcList.Contains(method))
                    rpcHash[5] = (byte)PhotonNetwork.PhotonServerSettings.RpcList.IndexOf(method);
                if (player == null)
                {
                    switch (target)
                    {
                        case RpcTarget.All:
                            return PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcHash,
                                new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.All,
                                    InterestGroup = photonView.Group
                                }, new SendOptions
                                {
                                    Reliability = true,
                                    DeliveryMode = DeliveryMode.ReliableUnsequenced,
                                    Encrypt = false
                                });
                            typeof(PhotonNetwork).GetMethod("ExecuteRpc", BindingFlags.Static | BindingFlags.NonPublic)
                                .Invoke(typeof(PhotonNetwork), new object[]
                                {
                                    rpcHash, PhotonNetwork.LocalPlayer
                                });
                            break;

                        case RpcTarget.Others:
                            return PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcHash,
                                new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.Others,
                                    InterestGroup = photonView.Group
                                }, new SendOptions
                                {
                                    Reliability = true,
                                    DeliveryMode = DeliveryMode.ReliableUnsequenced,
                                    Encrypt = false
                                });
                            break;

                        case RpcTarget.AllBuffered:
                            return PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcHash,
                                new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.All,
                                    InterestGroup = photonView.Group,
                                    CachingOption = EventCaching.AddToRoomCache
                                }, new SendOptions
                                {
                                    Reliability = true,
                                    DeliveryMode = DeliveryMode.ReliableUnsequenced,
                                    Encrypt = false
                                });
                            typeof(PhotonNetwork).GetMethod("ExecuteRpc", BindingFlags.Static | BindingFlags.NonPublic)
                                .Invoke(typeof(PhotonNetwork), new object[]
                                {
                                    rpcHash, PhotonNetwork.LocalPlayer
                                });
                            break;

                        case RpcTarget.OthersBuffered:
                            return PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcHash,
                                new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.Others,
                                    InterestGroup = photonView.Group,
                                    CachingOption = EventCaching.AddToRoomCache
                                }, new SendOptions
                                {
                                    Reliability = true,
                                    DeliveryMode = DeliveryMode.ReliableUnsequenced,
                                    Encrypt = false
                                });
                            break;

                        case RpcTarget.AllBufferedViaServer:
                            return PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcHash,
                                new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.All,
                                    InterestGroup = photonView.Group
                                }, new SendOptions
                                {
                                    Reliability = true,
                                    DeliveryMode = DeliveryMode.ReliableUnsequenced,
                                    Encrypt = false
                                });
                            if (PhotonNetwork.OfflineMode)
                                typeof(PhotonNetwork)
                                    .GetMethod("ExecuteRpc", BindingFlags.Static | BindingFlags.NonPublic).Invoke(
                                        typeof(PhotonNetwork), new object[]
                                        {
                                            rpcHash, PhotonNetwork.LocalPlayer
                                        });
                            break;

                        case RpcTarget.AllViaServer:
                            return PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcHash,
                                new RaiseEventOptions
                                {
                                    Receivers = ReceiverGroup.All,
                                    InterestGroup = photonView.Group,
                                    CachingOption = EventCaching.AddToRoomCache
                                }, new SendOptions
                                {
                                    Reliability = true,
                                    DeliveryMode = DeliveryMode.ReliableUnsequenced,
                                    Encrypt = false
                                });
                            if (PhotonNetwork.OfflineMode)
                                typeof(PhotonNetwork)
                                    .GetMethod("ExecuteRpc", BindingFlags.Static | BindingFlags.NonPublic).Invoke(
                                        typeof(PhotonNetwork), new object[]
                                        {
                                            rpcHash, PhotonNetwork.LocalPlayer
                                        });
                            break;
                    }
                }
                else
                {
                    if (PhotonNetwork.NetworkingClient.LocalPlayer.ActorNumber == player.ActorNumber)
                        typeof(PhotonNetwork).GetMethod("ExecuteRpc", BindingFlags.Static | BindingFlags.NonPublic)
                            .Invoke(typeof(PhotonNetwork), new object[]
                            {
                                rpcHash, PhotonNetwork.LocalPlayer
                            });
                    else
                        return PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(200, rpcHash,
                            new RaiseEventOptions
                            {
                                TargetActors = new[]
                                {
                                    player.ActorNumber
                                }
                            }, new SendOptions
                            {
                                Reliability = true,
                                DeliveryMode = DeliveryMode.ReliableUnsequenced,
                                Encrypt = false
                            });
                }
            }

            return false;
        }
    }
}
