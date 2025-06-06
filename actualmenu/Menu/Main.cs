using BepInEx;
using CSCore;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using WristMenu.Classes;
using WristMenu.Notifications;
using static Fusion.Sockets.NetBitBuffer;
using static WristMenu.Menu.Buttons;
using static WristMenu.Settings;

namespace WristMenu.Menu
{
    [HarmonyPatch(typeof(GorillaLocomotion.GTPlayer))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class Main : MonoBehaviour
    {
        // Constant

        public static bool ForceMenu;
        public static AudioSource source;

        public static void Prefix()
        {
            // Initialize Menu
                try
                {
                    if (!ForceMenu)
                    {

                        bool toOpen = (!rightHanded && ControllerInputPoller.instance.leftControllerSecondaryButton) || (rightHanded && ControllerInputPoller.instance.rightControllerSecondaryButton);
                        bool keyboardOpen = UnityInput.Current.GetKey(keyboardButton);

                        if (menu == null)
                        {
                            if (toOpen || keyboardOpen)
                            {
                                CreateMenu();
                                RecenterMenu(rightHanded, keyboardOpen);
                                if (reference == null)
                                {
                                    CreateReference(rightHanded);
                                }
                            }
                        }
                        else
                        {
                            if ((toOpen || keyboardOpen))
                            {
                                RecenterMenu(rightHanded, keyboardOpen);
                            }
                            else
                            {
                                GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(true);

                                Rigidbody comp = menu.AddComponent(typeof(Rigidbody)) as Rigidbody;
                                if (rightHanded)
                                {
                                    comp.velocity = GorillaLocomotion.GTPlayer.Instance.rightHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                                }
                                else
                                {
                                    comp.velocity = GorillaLocomotion.GTPlayer.Instance.leftHandCenterVelocityTracker.GetAverageVelocity(true, 0);
                                }

                                UnityEngine.Object.Destroy(menu, 2);
                                menu = null;

                                UnityEngine.Object.Destroy(reference);
                                reference = null;
                            }
                        } 
                    }
                }
                catch (Exception exc)
                {
                    UnityEngine.Debug.LogError(string.Format("{0} // Error initializing at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
                }

            // Constant
                try
                {
                    // Pre-Execution
                        if (fpsObject != null)
                        {
                            fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                        }

                        if (pingObject != null)
                        {
                            if (PhotonNetwork.IsConnected)
                            {
                                pingObject.text = "Ping: " + PhotonNetwork.GetPing();
                            }
                        }

                    // Execute Enabled mods
                        foreach (ButtonInfo[] buttonlist in buttons)
                        {
                            foreach (ButtonInfo v in buttonlist)
                            {
                                if (v.enabled)
                                {
                                    if (v.method != null)
                                    {
                                        try
                                        {
                                            v.method.Invoke();
                                        }
                                        catch (Exception exc)
                                        {
                                            UnityEngine.Debug.LogError(string.Format("{0} // Error with mod {1} at {2}: {3}", PluginInfo.Name, v.buttonText, exc.StackTrace, exc.Message));
                                        }
                                    }
                                }
                            }
                        }
                } catch (Exception exc)
                {
                    UnityEngine.Debug.LogError(string.Format("{0} // Error with executing mods at {1}: {2}", PluginInfo.Name, exc.StackTrace, exc.Message));
                }
        }

        // Functions
        public static void CreateMenu()
        {
            // Menu Holder
                menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(menu.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(menu.GetComponent<BoxCollider>());
                UnityEngine.Object.Destroy(menu.GetComponent<Renderer>());
                menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);

            // Menu Background
                menuBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(menuBackground.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(menuBackground.GetComponent<BoxCollider>());
                menuBackground.transform.parent = menu.transform;
                menuBackground.transform.rotation = Quaternion.identity;
                menuBackground.transform.localScale = menuSize;
                menuBackground.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
                menuBackground.transform.position = new Vector3(0.05f, 0f, 0f);

                RoundObj(menuBackground);

                ColorChanger colorChanger = menuBackground.AddComponent<ColorChanger>();
                colorChanger.colorInfo = backgroundColor;
                colorChanger.Start();

            // Canvas
                canvasObject = new GameObject();
                canvasObject.transform.parent = menu.transform;
                Canvas canvas = canvasObject.AddComponent<Canvas>();
                CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvasScaler.dynamicPixelsPerUnit = 2500f;

            // Title and FPS
                Text text = new GameObject
                {
                    transform =
                    {
                        parent = canvasObject.transform
                    }
                }.AddComponent<Text>();
                text.font = currentFont;
                text.text = PluginInfo.Name + " <color=grey>[</color><color=white>" + (pageNumber + 1).ToString() + "</color><color=grey>]</color>";
                text.fontSize = 1;
                text.color = textColors[0];
                text.supportRichText = true;
                //text.fontStyle = FontStyle.Italic;
                text.alignment = TextAnchor.MiddleCenter;
                text.resizeTextForBestFit = true;
                text.resizeTextMinSize = 0;
                RectTransform component = text.GetComponent<RectTransform>();
                component.localPosition = Vector3.zero;
                component.sizeDelta = new Vector2(0.24f, 0.03f);
                component.position = new Vector3(0.06f, 0f, 0.165f);
                component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                if (fpsCounter)
                {
                    fpsObject = new GameObject
                    {
                        transform =
                    {
                        parent = canvasObject.transform
                    }
                    }.AddComponent<Text>();
                    fpsObject.font = currentFont;
                    fpsObject.text = "FPS: " + Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
                    fpsObject.color = textColors[0];
                    fpsObject.fontSize = 1;
                    fpsObject.supportRichText = true;
                    //fpsObject.fontStyle = FontStyle.Italic;
                    fpsObject.alignment = TextAnchor.MiddleCenter;
                    fpsObject.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
                    fpsObject.resizeTextForBestFit = true;
                    fpsObject.resizeTextMinSize = 0;
                    RectTransform component2 = fpsObject.GetComponent<RectTransform>();
                    component2.localPosition = Vector3.zero;
                    component2.sizeDelta = new Vector2(0.26f, 0.02f);
                    component2.position = new Vector3(0.06f, 0.101f, 0.139f);
                    component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                }

                if (pingDisplay)
                {
                    pingObject = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    pingObject.font = currentFont;
                    pingObject.text = "Ping: " + PhotonNetwork.GetPing();
                    pingObject.color = textColors[0];
                    pingObject.fontSize = 1;
                    pingObject.supportRichText = true;
                    //fpsObject.fontStyle = FontStyle.Italic;
                    pingObject.alignment = TextAnchor.MiddleCenter;
                    pingObject.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
                    pingObject.resizeTextForBestFit = true;
                    pingObject.resizeTextMinSize = 0;
                    RectTransform component2 = pingObject.GetComponent<RectTransform>();
                    component2.localPosition = Vector3.zero;
                    component2.sizeDelta = new Vector2(0.26f, 0.02f);
                    component2.position = new Vector3(0.06f, -0.0963f, 0.139f);
                    component2.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                }

            // Buttons
                // Disconnect
                    if (disconnectButton)
                    {
                        GameObject disconnectbutton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        if (!UnityInput.Current.GetKey(KeyCode.Q))
                        {
                            disconnectbutton.layer = 2;
                        }
                        UnityEngine.Object.Destroy(disconnectbutton.GetComponent<Rigidbody>());
                        disconnectbutton.GetComponent<BoxCollider>().isTrigger = true;
                        disconnectbutton.transform.parent = menu.transform;
                        disconnectbutton.transform.rotation = Quaternion.identity;
                        disconnectbutton.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
                        disconnectbutton.transform.localPosition = new Vector3(0.56f, 0f, 0.6f);
                        disconnectbutton.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                        disconnectbutton.AddComponent<Classes.Button>().relatedText = "Disconnect";

                        RoundObj(disconnectbutton);

                        colorChanger = disconnectbutton.AddComponent<ColorChanger>();
                        colorChanger.colorInfo = buttonColors[0];
                        colorChanger.Start();

                        Text discontext = new GameObject
                        {
                            transform =
                            {
                                parent = canvasObject.transform
                            }
                        }.AddComponent<Text>();
                        discontext.text = "Disconnect";
                        discontext.font = currentFont;
                        discontext.fontSize = 1;
                        discontext.color = textColors[0];
                        discontext.alignment = TextAnchor.MiddleCenter;
                        discontext.resizeTextForBestFit = true;
                        discontext.resizeTextMinSize = 0;

                        RectTransform rectt = discontext.GetComponent<RectTransform>();
                        rectt.localPosition = Vector3.zero;
                        rectt.sizeDelta = new Vector2(0.2f, 0.03f);
                        rectt.localPosition = new Vector3(0.064f, 0f, 0.23f);
                        rectt.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
                    }

            // Page Buttons
            CreatePageButtons();
                    /*GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (!UnityInput.Current.GetKey(KeyCode.Q))
                    {
                        gameObject.layer = 2;
                    }
                    UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
                    gameObject.GetComponent<BoxCollider>().isTrigger = true;
                    gameObject.transform.parent = menu.transform;
                    gameObject.transform.rotation = Quaternion.identity;
                    gameObject.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
                    gameObject.transform.localPosition = new Vector3(0.56f, 0.65f, 0);
                    gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                    gameObject.AddComponent<Classes.Button>().relatedText = "PreviousPage";

                    RoundObj(gameObject);

                    colorChanger = gameObject.AddComponent<ColorChanger>();
                    colorChanger.colorInfo = buttonColors[0];
                    colorChanger.Start();

                    text = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    text.font = currentFont;
                    text.text = "<";
                    text.fontSize = 1;
                    text.color = textColors[0];
                    text.alignment = TextAnchor.MiddleCenter;
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 0;
                    component = text.GetComponent<RectTransform>();
                    component.localPosition = Vector3.zero;
                    component.sizeDelta = new Vector2(0.2f, 0.03f);
                    component.localPosition = new Vector3(0.064f, 0.195f, 0f);
                    component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

                    gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    if (!UnityInput.Current.GetKey(KeyCode.Q))
                    {
                        gameObject.layer = 2;
                    }
                    UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
                    gameObject.GetComponent<BoxCollider>().isTrigger = true;
                    gameObject.transform.parent = menu.transform;
                    gameObject.transform.rotation = Quaternion.identity;
                    gameObject.transform.localScale = new Vector3(0.09f, 0.2f, 0.9f);
                    gameObject.transform.localPosition = new Vector3(0.56f, -0.65f, 0);
                    gameObject.GetComponent<Renderer>().material.color = buttonColors[0].colors[0].color;
                    gameObject.AddComponent<Classes.Button>().relatedText = "NextPage";

                    colorChanger = gameObject.AddComponent<ColorChanger>();
                    colorChanger.colorInfo = buttonColors[0];
                    colorChanger.Start();

                    text = new GameObject
                    {
                        transform =
                        {
                            parent = canvasObject.transform
                        }
                    }.AddComponent<Text>();
                    text.font = currentFont;
                    text.text = ">";
                    text.fontSize = 1;
                    text.color = textColors[0];
                    text.alignment = TextAnchor.MiddleCenter;
                    text.resizeTextForBestFit = true;
                    text.resizeTextMinSize = 0;
                    component = text.GetComponent<RectTransform>();
                    component.localPosition = Vector3.zero;
                    component.sizeDelta = new Vector2(0.2f, 0.03f);
                    component.localPosition = new Vector3(0.064f, -0.195f, 0f);
                    component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));*/

                // Mod Buttons
                    ButtonInfo[] activeButtons = buttons[buttonsType].Skip(pageNumber * buttonsPerPage).Take(buttonsPerPage).ToArray();
                    for (int i = 0; i < activeButtons.Length; i++)
                    {
                        CreateButton(i * 0.1f, activeButtons[i]);
            }
        }

        public static void RoundObj(GameObject toRound)
        {
            float bevel = 0.02f;
            Transform parent = menu.transform;

            Renderer toRoundRenderer = toRound.GetComponent<Renderer>();
            Vector3 pos = toRound.transform.localPosition;
            Vector3 scale = toRound.transform.localScale;

            Vector3 baseAScale = scale + new Vector3(0f, bevel * -2.55f, 0f);
            Vector3 baseBScale = scale + new Vector3(0f, 0f, -bevel * 2f);
            Vector3 roundCornerScale = new Vector3(bevel * 2.55f, scale.x / 2f, bevel * 2f);

            Vector3 top = new Vector3(0f, (scale.y / 2f) - (bevel * 1.275f), (scale.z / 2f) - bevel);
            Vector3 bottom = new Vector3(0f, -(scale.y / 2f) + (bevel * 1.275f), (scale.z / 2f) - bevel);
            Vector3 topBack = new Vector3(0f, (scale.y / 2f) - (bevel * 1.275f), -(scale.z / 2f) + bevel);
            Vector3 bottomBack = new Vector3(0f, -(scale.y / 2f) + (bevel * 1.275f), -(scale.z / 2f) + bevel);

            GameObject CreatePiece(PrimitiveType type, Vector3 localPosition, Vector3 localScale, Quaternion rotation)
            {
                GameObject obj = GameObject.CreatePrimitive(type);
                obj.transform.SetParent(parent, false);
                obj.transform.localPosition = pos + localPosition;
                obj.transform.localRotation = rotation;
                obj.transform.localScale = localScale;

                Renderer rend = obj.GetComponent<Renderer>();
                rend.enabled = toRoundRenderer.enabled;

                UnityEngine.Object.Destroy(obj.GetComponent<Collider>());
                return obj;
            }

            Quaternion cylinderRot = Quaternion.Euler(0f, 0f, 90f);

            GameObject[] toChange =
            {
                CreatePiece(PrimitiveType.Cube, Vector3.zero, baseAScale, Quaternion.identity),
                CreatePiece(PrimitiveType.Cube, Vector3.zero, baseBScale, Quaternion.identity),
                CreatePiece(PrimitiveType.Cylinder, top, roundCornerScale, cylinderRot),
                CreatePiece(PrimitiveType.Cylinder, bottom, roundCornerScale, cylinderRot),
                CreatePiece(PrimitiveType.Cylinder, topBack, roundCornerScale, cylinderRot),
                CreatePiece(PrimitiveType.Cylinder, bottomBack, roundCornerScale, cylinderRot)
            };

            foreach (GameObject obj in toChange)
            {
                ClampColor cc = obj.AddComponent<ClampColor>();
                cc.targetRenderer = toRoundRenderer;
            }

            toRoundRenderer.enabled = false;
        }


        public static void CreatePageButtons()
        {
            // Previous Page
            GameObject PreviousPage = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                PreviousPage.layer = 2;
            }
            UnityEngine.Object.Destroy(PreviousPage.GetComponent<Rigidbody>());
            PreviousPage.GetComponent<BoxCollider>().isTrigger = true;
            PreviousPage.transform.parent = menu.transform;
            PreviousPage.transform.rotation = Quaternion.identity;
            PreviousPage.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
            PreviousPage.transform.localPosition = new Vector3(0.56f, 0f, 0.28f);
            PreviousPage.AddComponent<Classes.Button>().relatedText = "PreviousPage";

            ColorChanger colorChanger = PreviousPage.AddComponent<ColorChanger>();
            colorChanger.colorInfo = buttonColors[0];
            colorChanger.Start();

            RoundObj(PreviousPage);

            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = "◀";
            text.supportRichText = true;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            //text.fontStyle = FontStyle.Italic;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(9f, 0.015f);
            component.localPosition = new Vector3(.064f, 0, .111f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));

            // Next Page

            GameObject NextPage = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                NextPage.layer = 2;
            }
            UnityEngine.Object.Destroy(NextPage.GetComponent<Rigidbody>());
            NextPage.GetComponent<BoxCollider>().isTrigger = true;
            NextPage.transform.parent = menu.transform;
            NextPage.transform.rotation = Quaternion.identity;
            NextPage.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
            NextPage.transform.localPosition = new Vector3(0.56f, 0f, 0.18f);
            NextPage.AddComponent<Classes.Button>().relatedText = "NextPage";

            ColorChanger colorChanger1 = NextPage.AddComponent<ColorChanger>();
            colorChanger1.colorInfo = buttonColors[0];
            colorChanger1.Start();

            RoundObj(NextPage);

            Text text1 = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text1.font = currentFont;
            text1.text = "▶";
            text1.supportRichText = true;
            text1.fontSize = 1;
            text1.alignment = TextAnchor.MiddleCenter;
            //text.fontStyle = FontStyle.Italic;
            text1.resizeTextForBestFit = true;
            text1.resizeTextMinSize = 0;
            RectTransform component1 = text1.GetComponent<RectTransform>();
            component1.localPosition = Vector3.zero;
            component1.sizeDelta = new Vector2(9f, 0.015f);
            component1.localPosition = new Vector3(0.064f, 0f, 0.0725f);
            component1.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        public static void CreateButton(float offset, ButtonInfo method)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (!UnityInput.Current.GetKey(KeyCode.Q))
            {
                gameObject.layer = 2;
            }
            UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.transform.parent = menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(0.09f, 0.9f, 0.08f);
            gameObject.transform.localPosition = new Vector3(0.56f, 0f, 0.08f - offset);
            gameObject.AddComponent<Classes.Button>().relatedText = method.buttonText;

            if (method.enabled)
            {
                gameObject.GetComponent<Renderer>().material.color = new Color32(119, 0, 255, 255);
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.black;
            }

            RoundObj(gameObject);

            Text text = new GameObject
            {
                transform =
                {
                    parent = canvasObject.transform
                }
            }.AddComponent<Text>();
            text.font = currentFont;
            text.text = method.buttonText;
            if (method.overlapText != null)
            {
                text.text = method.overlapText;
            }
            text.supportRichText = true;
            text.fontSize = 1;
            if (method.enabled)
            {
                text.color = textColors[1];
            }
            else
            {
                text.color = textColors[0];
            }
            text.alignment = TextAnchor.MiddleCenter;
            //text.fontStyle = FontStyle.Italic;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(9f, 0.015f);
            component.localPosition = new Vector3(0.064f, 0f, 0.0341f - offset / 2.6f);
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
        }

        public static void playsound(string path)
        {
            using (WWW www = new WWW("file://" + path.Replace("\\", "/")))
            {
                AudioClip clip = www.GetAudioClip(false, false, AudioType.WAV);

                if (clip == null)
                {
                    Debug.LogError("failed to load clip");
                    return;
                }

                GameObject obj = new GameObject("PopSoundPlayer");
                DontDestroyOnLoad(obj);

                source = obj.AddComponent<AudioSource>();
                source.clip = clip;
                source.Play();
            }
        }

        public static void RecreateMenu()
        {
            if (menu != null)
            {
                UnityEngine.Object.Destroy(menu);
                menu = null;

                CreateMenu();
                RecenterMenu(rightHanded, UnityInput.Current.GetKey(keyboardButton));
            }
        }

        public static void RecenterMenu(bool isRightHanded, bool isKeyboardCondition)
        {
            if (!isKeyboardCondition)
            {
                if (!isRightHanded)
                {
                    menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                }
                else
                {
                    menu.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    Vector3 rotation = GorillaTagger.Instance.rightHandTransform.rotation.eulerAngles;
                    rotation += new Vector3(0f, 0f, 180f);
                    menu.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                try
                {
                    TPC = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                }
                catch { }

                GameObject.Find("Shoulder Camera").transform.Find("CM vcam1").gameObject.SetActive(false);

                if (TPC != null)
                {
                    TPC.transform.position = new Vector3(-67.9299f, 11.9144f, -84.2019f);
                    TPC.transform.rotation = Quaternion.identity;
                    menu.transform.parent = TPC.transform;
                    menu.transform.position = (TPC.transform.position + (Vector3.Scale(TPC.transform.forward, new Vector3(0.5f, 0.5f, 0.5f)))) + (Vector3.Scale(TPC.transform.up, new Vector3(-0.02f, -0.02f, -0.02f)));
                    Vector3 rot = TPC.transform.rotation.eulerAngles;
                    rot = new Vector3(rot.x - 90, rot.y + 90, rot.z);
                    menu.transform.rotation = Quaternion.Euler(rot);

                    if (reference != null)
                    {
                        if (Mouse.current.leftButton.isPressed)
                        {
                            Ray ray = TPC.ScreenPointToRay(Mouse.current.position.ReadValue());
                            RaycastHit hit;
                            bool worked = Physics.Raycast(ray, out hit, 100);
                            if (worked)
                            {
                                Classes.Button collide = hit.transform.gameObject.GetComponent<Classes.Button>();
                                if (collide != null)
                                {
                                    collide.OnTriggerEnter(buttonCollider);
                                }
                            }
                        }
                        else
                        {
                            reference.transform.position = new Vector3(999f, -999f, -999f);
                        }
                    }
                }
            }
        }

        public static void CreateReference(bool isRightHanded)
        {
            reference = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (isRightHanded)
            {
                reference.transform.parent = GorillaTagger.Instance.leftHandTransform;
            }
            else
            {
                reference.transform.parent = GorillaTagger.Instance.rightHandTransform;
            }
            reference.GetComponent<Renderer>().material.color = backgroundColor.colors[0].color;
            reference.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            reference.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            buttonCollider = reference.GetComponent<SphereCollider>();

            ColorChanger colorChanger = reference.AddComponent<ColorChanger>();
            colorChanger.colorInfo = backgroundColor;
            colorChanger.Start();
        }

        public static void Toggle(string buttonText)
        {
            int lastPage = ((buttons[buttonsType].Length + buttonsPerPage - 1) / buttonsPerPage) - 1;
            if (buttonText == "PreviousPage")
            {
                pageNumber--;
                if (pageNumber < 0)
                {
                    pageNumber = lastPage;
                }
            } else
            {
                if (buttonText == "NextPage")
                {
                    pageNumber++;
                    if (pageNumber > lastPage)
                    {
                        pageNumber = 0;
                    }
                } else
                {
                    ButtonInfo target = GetIndex(buttonText);
                    if (target != null)
                    {
                        if (target.isTogglable)
                        {
                            target.enabled = !target.enabled;
                            if (target.enabled)
                            {
                                NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " + target.toolTip);
                                if (target.enableMethod != null)
                                {
                                    try { target.enableMethod.Invoke(); } catch { }
                                }
                            }
                            else
                            {
                                NotifiLib.SendNotification("<color=grey>[</color><color=red>DISABLE</color><color=grey>]</color> " + target.toolTip);
                                if (target.disableMethod != null)
                                {
                                    try { target.disableMethod.Invoke(); } catch { }
                                }
                            }
                        }
                        else
                        {
                            NotifiLib.SendNotification("<color=grey>[</color><color=green>ENABLE</color><color=grey>]</color> " + target.toolTip);
                            if (target.method != null)
                            {
                                try { target.method.Invoke(); } catch { }
                            }
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogError(buttonText + " does not exist");
                    }
                }
            }
            RecreateMenu();
        }

        public static GradientColorKey[] GetSolidGradient(Color color)
        {
            return new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) };
        }

        public static ButtonInfo GetIndex(string buttonText)
        {
            foreach (ButtonInfo[] buttons in Menu.Buttons.buttons)
            {
                foreach (ButtonInfo button in buttons)
                {
                    if (button.buttonText == buttonText)
                    {
                        return button;
                    }
                }
            }

            return null;
        }

        // Variables
            // Important
                // Objects
                    public static GameObject menu;
                    public static GameObject menuBackground;   
                    public static GameObject reference;
                    public static GameObject canvasObject;

                    public static SphereCollider buttonCollider;
                    public static Camera TPC;
                    public static Text fpsObject;
                    public static Text pingObject;

        // Data
        public static int pageNumber = 0;
            public static int buttonsType = 0;
    }
}
