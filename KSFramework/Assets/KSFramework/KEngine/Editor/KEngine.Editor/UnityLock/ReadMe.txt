UnityLock
Copyright 2012 Nick Gravelyn

UnityLock is a small utility that allows you to nearly completely lock any game objects in your scene from being edited.
The idea is that you can identify pieces of your scene that you want to prevent from being accidentally moved or deleted,
or perhaps from being edited entirely. These objects may be your primary scene geometry, some game manager type object, or
anything else.

If you've ever accidentally selected and deleted an object, UnityLock can help you prevent that.
If you've ever accidentally selected and moved an object, UnityLock can help you prevent that.

UnityLock adds a "UnityLock" submenu to the GameObject menu in Unity. Inside this submenu are four menu items:

  Lock Game Object
  Lock Game Object and Children (Shortcut: Cmd-Shift-L for OS X or Control-Shift-L for Windows)
  Unlock Game Object
  Unlock Game Object and Children (Shortcut: Cmd-Shift-U for OS X or Control-Shift-U for Windows)

These all behave the same way with regards to the objects, however you can choose whether you want to lock only the selected
game object(s) or if you'd like to lock their children as well. Given that most people want to lock an entire hierarchy, those
are the items that have the convenient keyboard shortcuts.

Locking a game object has the follow effects in the editor:

- Disables all 3D scene transform controls. Users cannot move, rotate, or scale the transform.
- Disables all inspectors for all components on the object.
- Prevents adding or removing components to/from the object.
- Prevents the object from being deleted from the scene.
- Prevents editing any top-level game object properties, such as the name, tag, layer, and static flags/toggle.

Locking an object does NOT prevent users from changing the parent-child hierarchy of an object. This is a limitation of Unity
as UnityLock sets the object to not be editable however Unity still allows the parent-child hierarchy to change in the editor.

There are also some options available if you wish to customize the behavior of UnityLock. You will find a preferences window
available in the Window menu which exposes all options and a description of what each one does.


HISTORY:

1.1:
 - Added icon to hierarchy view for quick identification of locked objects.
 - Fixed method for unlocking objects that fixes bugs in hierarchies when you unlock one object in the tree then do an
   "Unlock Game Object and Children" on a parent object.
 - Added undo/redo support for lock operations fixing a bug where if you, say, moved an object, locked it, and hit undo a couple
   times, it'd try to undo the move but fail.
 - Added feature to suppress scene view selection for left click. Selection rectangles can't be suppressed for all types, notably
   mesh objects are still selectable with a selection rectangle.
 - Added a preferences window to control the new features available in the Window menu.

1.0:
 - Initial release.