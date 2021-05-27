using KSFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
using Object = UnityEngine.Object;

namespace KEngine.Editor
{
    /// <summary>
    /// Author：qingqing.zhao (569032731@qq.com)
    /// Date：2020/12/7 20:12
    /// Desc：   扩展UI菜单，包括重写Unity的UI右键菜单
    ///          如果遇到同名菜单，则在后面添加/创建，解决Unity的编译Warn: annot add menu item xxx.
    ///          来源：UnityEditor.UI/UI/MenuOptions.cs，如果没有 UnityEditor.DefaultControls(需要Unity2018.4)，或从对应的Unity版本UGUI源码中复制实现
    /// </summary>
    public class KUIMenu
    {
        private const string menu = "GameObject/UI/";

        #region KSFramework Extension


        
        [MenuItem(menu + "EmptyImage(空Image用于点击)", false, 1001)]
        public static void AddEmptyImage(MenuCommand menuCommand)
        {
            var go = CreateUIElementRoot("EmptyImage", menuCommand, s_ThickGUIElementSize);
            var image = go.AddComponent<EmptyImage>();
            image.raycastTarget = true;
        }

        [MenuItem(menu + "ScrollRect(水平列表)", false, 1002)]
        static public void AddScrollRectHorizontal(MenuCommand menuCommand)
        {
            AddScrollRect(menuCommand, typeof(HorizontalLayoutGroup));
        }

        [MenuItem(menu + "ScrollRect(垂直列表)", false, 1003)]
        static public void AddScrollRectVertical(MenuCommand menuCommand)
        {
            AddScrollRect(menuCommand, typeof(VerticalLayoutGroup));
        }

        [MenuItem(menu + "ScrollRect(格子排列)", false, 1004)]
        static public void AddScrollRectGrid(MenuCommand menuCommand)
        {
            AddScrollRect(menuCommand, typeof(GridLayoutGroup));
        }

        static public void AddScrollRect<T>(MenuCommand menuCommand, T type)
        {
            var go = CreateUIElementRoot("ScrollRect", menuCommand, ScrollRectSize);
            var scrollRect = go.AddComponent<ScrollRect>();
            var rect = go.GetComponent<RectTransform>();
            go.AddComponent<EmptyImage>().raycastTarget = true;
            go.AddComponent<RectMask2D>();

            //Create Content
            GameObject content = new GameObject("Content");
            GameObjectUtility.SetParentAndAlign(content, go);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.sizeDelta = ScrollRectSize;
            SetPositionVisibleinSceneView(rect, contentRect);
            scrollRect.content = contentRect;
            
            //Create Child
            GameObject item = new GameObject("Image");
            var itemRect = item.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(100,100);
            GameObjectUtility.SetParentAndAlign(item, content);
            SetPositionVisibleinSceneView(rect, itemRect);
            item.AddComponent<Image>();
            
            if (type.GetHashCode() == typeof(HorizontalLayoutGroup).GetHashCode())
            {
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
                var layoutGroup = content.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.spacing = 10;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
                content.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else if (type.GetHashCode() == typeof(VerticalLayoutGroup).GetHashCode())
            {
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                var layoutGroup = content.AddComponent<VerticalLayoutGroup>();
                layoutGroup.spacing = 10;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
                content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                //顶对齐
                contentRect.SetAnchor(AnchorType.StretchTop);
            }
            else if (type.GetHashCode() == typeof(GridLayoutGroup).GetHashCode())
            {
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                var layoutGroup = content.AddComponent<GridLayoutGroup>();
                layoutGroup.cellSize = new Vector3(100, 100);
                layoutGroup.spacing = new Vector2(10, 10);
            }
        }

        #endregion

        #region default ui resources
        
        private const string kUILayerName = "UI";

        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpritePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath = "UI/Skin/UIMask.psd";
        private const float kWidth = 160f;
        private const float kThickHeight = 30f;

        private static Vector2 s_ThickGUIElementSize = new Vector2(kWidth, kThickHeight);
        private static Vector2 ScrollRectSize = new Vector2(400, 100);
        private static Color   s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        private static Sprite _buttonSprite;
        private static Sprite buttonSprite
        {
            get
            {
                if (_buttonSprite == null)
                    _buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/{KEngineDef.ResourcesEditDir}/UI/atlas_common/btn_panel_02.png");
                return _buttonSprite;
            }
        }
        private static Sprite _closeSprite;
        private static Sprite closeSprite
        {
            get
            {
                if (_closeSprite == null)
                    _closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/{KEngineDef.ResourcesEditDir}/UI/atlas_common/btn_win_close.png");
                return _closeSprite;
            }
        }
        static private DefaultControls.Resources s_StandardResources;

        static private DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
            }

            return s_StandardResources;
        }
        
        #endregion
        
        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera,
                out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            bool explicitParentChoice = true;
            if (parent == null)
            {
                parent = GetOrCreateCanvasGameObject();
                explicitParentChoice = false;

                // If in Prefab Mode, Canvas has to be part of Prefab contents,
                // otherwise use Prefab root instead.
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }

            if (parent.GetComponentInParent<Canvas>() == null)
            {
                // Create canvas under context GameObject,
                // and make that be the parent which UI element is added under.
                GameObject canvas = CreateNewUI();
                canvas.transform.SetParent(parent.transform, false);
                parent = canvas;
            }

            // Setting the element to be a child of an element already in the scene should
            // be sufficient to also move the element to that scene.
            // However, it seems the element needs to be already in its destination scene when the
            // RegisterCreatedObjectUndo is performed; otherwise the scene it was created in is dirtied.
            SceneManager.MoveGameObjectToScene(element, parent.scene);

            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);

            if (element.transform.parent == null)
            {
                Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            }

            GameObjectUtility.EnsureUniqueNameForSibling(element);

            // We have to fix up the undo name since the name of the object was only known after reparenting it.
            Undo.SetCurrentGroupName("Create " + element.name);

            GameObjectUtility.SetParentAndAlign(element, parent);
            if (!explicitParentChoice) // not a context click, so center in sceneview
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        // Graphic elements

        [MenuItem("GameObject/UI/Text/创建", false, 2000)]
        static public void AddText(MenuCommand menuCommand)
        {
            //GameObject go = DefaultControls.CreateText(GetStandardResources());
            //PlaceUIElementRoot(go, menuCommand);
            var go = CreateUIElementRoot("Text", menuCommand, s_ThickGUIElementSize);
            SetDefaultTextValues(go);
        }
        
        static  MyText SetDefaultTextValues(GameObject go,string langId = null)
        {
            var text = go.AddComponent<MyText>();
            if (!string.IsNullOrEmpty(langId))
            {
                text.LangId = langId;
                text.text = I18N.Get(langId);
                text.UseLangId = true;
            }
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.fontSize = 18;
            //lbl.font = Resources.Load<Font>("xxx"); //TODO 设置游戏中的字体
            return text;
        }
        
        [MenuItem("GameObject/UI/Image/创建", false, 2001)]
        static public void AddImage(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateImage(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
        }

        [MenuItem("GameObject/UI/Raw Image/创建", false, 2002)]
        static public void AddRawImage(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateRawImage(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
            var image = go.GetComponent<RawImage>();
            image.raycastTarget = false;
        }
        
        /// <summary>
        /// Create the basic UI button.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Button
        ///         -Text
        /// </remarks>
        [MenuItem(menu+"/Button/创建", false, 2030)]
        static public void AddButton(MenuCommand menuCommand)
        {
            AddButton(menuCommand, buttonSprite, "common_ok");
        }
        
        [MenuItem(menu+"/Button/创建关闭按钮", false, 2030)]
        static public void AddCloseButton(MenuCommand menuCommand)
        {
            AddButton(menuCommand, closeSprite, null);
        }
        
        static public Button AddButton(MenuCommand menuCommand, Sprite sprite, string langId = "")
        {
            var buttonRoot = CreateUIElementRoot("Button", menuCommand, s_ThickGUIElementSize);
            Image image = buttonRoot.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = s_DefaultSelectableColor;
            image.SetNativeSize();
            Button button = buttonRoot.AddComponent<Button>();
            SetDefaultColorTransitionValues(button);
            
            //text
            if (langId == null) return button;
            GameObject childText = CreateUIObject("Text", buttonRoot);
            SetDefaultTextValues(childText,langId);

            RectTransform textRectTransform = childText.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            return button;
        }
        
        //[MenuItem("GameObject/UI/Toggle", false, 2031)]
        static public void AddToggle(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateToggle(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // Slider and Scrollbar modify a number

        //[MenuItem("GameObject/UI/Slider", false, 2033)]
        static public void AddSlider(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateSlider(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        //[MenuItem("GameObject/UI/Scrollbar", false, 2034)]
        static public void AddScrollbar(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateScrollbar(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // More advanced controls below

        //[MenuItem("GameObject/UI/Dropdown", false, 2035)]
        static public void AddDropdown(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateDropdown(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        //[MenuItem("GameObject/UI/Input Field", false, 2036)]
        public static void AddInputField(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateInputField(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // Containers

        //[MenuItem("GameObject/UI/Canvas", false, 2060)]
        static public void AddCanvas(MenuCommand menuCommand)
        {
            var go = CreateNewUI();
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            if (go.transform.parent as RectTransform)
            {
                RectTransform rect = go.transform as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }

            Selection.activeGameObject = go;
        }

        //[MenuItem("GameObject/UI/Panel", false, 2061)]
        static public void AddPanel(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreatePanel(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);

            // Panel is special, we need to ensure there's no padding after repositioning.
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        //[MenuItem("GameObject/UI/Scroll View", false, 2062)]
        static public void AddScrollView(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateScrollView(GetStandardResources());
            PlaceUIElementRoot(go, menuCommand);
        }

        // Helper methods
        static public GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(kUILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            // Works for all stages.
            StageUtility.PlaceGameObjectInCurrentStage(root);
            bool customScene = false;
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                root.transform.SetParent(prefabStage.prefabContentsRoot.transform, false);
                customScene = true;
            }

            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // If there is no event system add one...
            // No need to place event system in custom scene as these are temporary anyway.
            // It can be argued for or against placing it in the user scenes,
            // but let's not modify scene user is not currently looking at.
            if (!customScene)
                CreateEventSystem(false);
            return root;
        }

        //[MenuItem("GameObject/UI/Event System", false, 2100)]
        public static void CreateEventSystem(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            CreateEventSystem(true, parent);
        }

        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            StageHandle stage = parent == null ? StageUtility.GetCurrentStageHandle() : StageUtility.GetStageHandle(parent);
            var esys = stage.FindComponentOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                if (parent == null)
                    StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
                else
                    GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }

        // Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        static public GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (IsValidCanvas(canvas))
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use any valid canvas.
            // We have to find all loaded Canvases, not just the ones in main scenes.
            Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            for (int i = 0; i < canvasArray.Length; i++)
                if (IsValidCanvas(canvasArray[i]))
                    return canvasArray[i].gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

        static bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                return false;

            // It's important that the non-editable canvas from a prefab scene won't be rejected,
            // but canvases not visible in the Hierarchy at all do. Don't check for HideAndDontSave.
            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
                return false;

            if (StageUtility.GetStageHandle(canvas.gameObject) != StageUtility.GetCurrentStageHandle())
                return false;

            return true;
        }

        #region ugui sources code
static public T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null)
                return null;

            T comp = null;
            Transform t = go.transform;
            while (t != null && comp == null)
            {
                comp = t.GetComponent<T>();
                t = t.parent;
            }

            return comp;
        }

        // Helper function that returns the selected root object.
        static public GameObject GetParentActiveCanvasInSelection(bool createIfMissing)
        {
            GameObject go = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if ots parents
            Canvas p = (go != null) ? FindInParents<Canvas>(go) : null;
            // Only use active objects
            if (p != null && p.gameObject.activeInHierarchy)
                go = p.gameObject;

            // No canvas in selection or its parents? Then use just any canvas.
            if (go == null)
            {
                Canvas canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
                if (canvas != null)
                    go = canvas.gameObject;
            }

            // No canvas present? Create a new one.
            if (createIfMissing && go == null)
                go = CreateNewUI();

            return go;
        }

        private static GameObject CreateUIElementRoot(string name, MenuCommand menuCommand, Vector2 size)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || FindInParents<Canvas>(parent) == null)
            {
                parent = GetParentActiveCanvasInSelection(true);
            }

            GameObject child = new GameObject(name);

            Undo.RegisterCreatedObjectUndo(child, "Create " + name);
            Undo.SetTransformParent(child.transform, parent.transform, "Parent " + child.name);
            GameObjectUtility.SetParentAndAlign(child, parent);

            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            if (parent != menuCommand.context) // not a context click, so center in sceneview
            {
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), rectTransform);
            }

            Selection.activeGameObject = child;
            return child;
        }
        
        public static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }
        public static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        public static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
        
        public static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
        }
        #endregion
    }
}