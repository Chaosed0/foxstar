using UnityEngine;
using System.Collections;

public class SplitHelper {
    private class SplitPair {
        public Camera camera;
        public HUD hud;
        public SplitPair(Camera camera, HUD hud) {
            this.camera = camera;
            this.hud = hud;
        }
    }

    public enum Orientation {
        CENTER,
        LEFT,
        RIGHT,
        TOP_LEFT,
        TOP_RIGHT,
        BOT_LEFT,
        BOT_RIGHT
    }

    private SplitPair topLeft = null;
    private SplitPair topRight = null;
    private SplitPair botLeft = null;
    private SplitPair botRight = null;

    public void addCamera(Camera camera, HUD hud) {
        SplitPair pair = new SplitPair(camera, hud);

        Canvas hudCanvas = hud.GetComponent<Canvas>();
        hudCanvas.worldCamera = camera;

        if (topLeft == null) {
            topLeft = pair;
        } else if (topRight == null) {
            topRight = pair;
            SetOrientation(topLeft, SplitHelper.Orientation.LEFT);
            SetOrientation(topRight, SplitHelper.Orientation.RIGHT);
        } else if (botLeft == null) {
            botLeft = pair;
            SetOrientation(topLeft, SplitHelper.Orientation.TOP_LEFT);
            SetOrientation(botLeft, SplitHelper.Orientation.BOT_LEFT);
        } else if (botRight == null) {
            botRight = pair;
            SetOrientation(topRight, SplitHelper.Orientation.TOP_RIGHT);
            SetOrientation(botRight, SplitHelper.Orientation.BOT_RIGHT);
        } else {
            Debug.Log("WARNING: Too many cameras, SplitHelper only supports 4 cameras");
        }
    }

    public void Finish() {
        if (topLeft != null) {
            topLeft.hud.Finish();
        }
        if (topRight != null) {
            topRight.hud.Finish();
        }
        if (botLeft != null) {
            botLeft.hud.Finish();
        }
        if (botRight != null) {
            botRight.hud.Finish();
        }
    }

    private void SetOrientation(SplitPair pair, Orientation orientation) {
        switch (orientation) {
            case Orientation.CENTER:
                pair.camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                break;
            case Orientation.LEFT:
                pair.camera.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
                break;
            case Orientation.RIGHT:
                pair.camera.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
                break;
            case Orientation.TOP_LEFT:
                pair.camera.rect = new Rect(0.0f, 0.5f, 0.5f, 0.5f);
                break;
            case Orientation.TOP_RIGHT:
                pair.camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                break;
            case Orientation.BOT_LEFT:
                pair.camera.rect = new Rect(0.0f, 0.0f, 0.5f, 0.5f);
                break;
            case Orientation.BOT_RIGHT:
                pair.camera.rect = new Rect(0.5f, 0.0f, 0.5f, 0.5f);
                break;
        }
        pair.hud.SetOrientation(orientation);
    }
}
