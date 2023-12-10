using KSP.Game;
using KSP.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace CommunityFixes.Fix.VABRedoTooltipFIx;

[Fix("VAB Redo tooltip offset fix")]
public class VABRedoTooltipFix: BaseFix
{
    private readonly Action<MessageCenterMessage> _fixCallback = _ => FixVABRedoTooltip();
    
    public override void OnInitialized()
    {
        Messages.Subscribe<OABLoadedMessage>(_fixCallback);
    }

    // This fix patches the VAB Redo button tooltip appearing above and to the right
    // of the button, instead of on the same line. It does this by copying the undo button texture
    // and flipping it. Since the actual texture is protected, we need to use the GPU to render the image in
    // a flipped position, and then save that to a new sprite that we replace the redo image with.
    public static void FixVABRedoTooltip()
    {
        const string undoButton = "/OAB(Clone)/HUDSpawner/HUD/widget_ToolBar/GRP-Undo-Redo/KSP2ButtonText_ToolsBar-Undo";
        const string redoButton = "/OAB(Clone)/HUDSpawner/HUD/widget_ToolBar/GRP-Undo-Redo/KSP2ButtonText_ToolsBar-Redo";

        var undoGameObject = GameObject.Find(undoButton);
        if (undoGameObject == null)
        {
            return;
        }

        var undoImage = undoGameObject.GetComponent<Image>();
        var oldSprite = undoImage.sprite;
        var tex = oldSprite.texture;
            
        // Original texture is readonly, so we have to render it through Graphics.Blit
        RenderTexture renderTex = RenderTexture.GetTemporary(
            tex.width,
            tex.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);
            
        // Flip X Axis
        var scale = new Vector2(-1, 1);
        var offset = new Vector2(1, 0);
            
        Graphics.Blit(tex, renderTex, scale, offset);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(tex.width, tex.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        
        // Make sure Unity doesn't anti-alias the pixel graphics
        readableText.filterMode = FilterMode.Point;
            
        var newSprite = Sprite.Create(
            readableText,
            oldSprite.rect,
            oldSprite.pivot
        );
            
        var redoGameObject = GameObject.Find(redoButton);
        if (redoGameObject == null)
        {
            return;
        }

        // Undo the negative y axis scaling to fix the tooltip bug
        redoGameObject.transform.localScale = new Vector3(1, 1, 1);
        
        // Replace the texture with our custom flipped one to prevent an upside down image
        var redoGameObjectImage = redoGameObject.GetComponent<Image>();
        redoGameObjectImage.sprite = newSprite;
    }
}