using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ImageMask : MonoBehaviour
{
	public RawImage targetImage;
	public Shader shaderMask;
	public Shader shaderEraser;
	public Texture brush;
	public float eraserSize = .5f;

	private RenderTexture renderTexture;
	private Material eraserMaterial;
	private bool firstFrame;
	private Vector2? newHolePosition;
	private Vector2 d;

	void Init()
	{
		// attach camera
		Camera c = gameObject.AddComponent<Camera> ();
		c.clearFlags = CameraClearFlags.Depth;
		c.cullingMask = 0;
		c.orthographic = true;
		c.orthographicSize = 1f;
		renderTexture = new RenderTexture (256, 256, 24);
		renderTexture.name = "123";
		c.targetTexture = renderTexture;
		// attach main material
		targetImage.material = new Material (shaderMask);
		targetImage.material.SetTexture ("_MainTex", targetImage.texture);
		targetImage.material.SetTexture ("_MaskTex", renderTexture);
		// attach eraser material
		eraserMaterial = new Material (shaderEraser);
		eraserMaterial.SetTexture ("_MainTex", brush);

		// TODO: sizeDelta isn't works if anchor = "match parent"
		d = targetImage.rectTransform.sizeDelta;
	}

	private void CutHole(Vector2 position01)
	{
		Rect textureRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		Rect positionRect = new Rect(
			position01.x - eraserSize/2 * (d.y/d.x), position01.y - eraserSize/2, 
			eraserSize * (d.y/d.x), eraserSize
		);
		RenderTexture old = RenderTexture.active;
		RenderTexture.active = renderTexture;
		GL.PushMatrix();
		GL.LoadOrtho();
        for (int i = 0; i < eraserMaterial.passCount; i++)
		{
			eraserMaterial.SetPass(i);
			GL.Begin(GL.QUADS);
			GL.Color(Color.white);
			GL.TexCoord2(textureRect.xMin, textureRect.yMax);
			GL.Vertex3(positionRect.xMin, positionRect.yMax, 0.0f);
			GL.TexCoord2(textureRect.xMax, textureRect.yMax);
			GL.Vertex3(positionRect.xMax, positionRect.yMax, 0.0f);
			GL.TexCoord2(textureRect.xMax, textureRect.yMin);
			GL.Vertex3(positionRect.xMax, positionRect.yMin, 0.0f);
			GL.TexCoord2(textureRect.xMin, textureRect.yMin);
			GL.Vertex3(positionRect.xMin, positionRect.yMin, 0.0f);
			GL.End();
		}
		GL.PopMatrix();

		RenderTexture.active = old;

	}

	public void Start()
	{
		Init ();
		firstFrame = true;

	}
	// TODO: function isn't call if you drag from another image
	public void Draw(BaseEventData bed){
		PointerEventData ped = bed as PointerEventData;
		// convert to local position
		Vector2 localCursor;
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(targetImage.GetComponent<RectTransform>(), ped.position, ped.pressEventCamera, out localCursor))
			return;
		// change anchor from middle to bottom-left
		localCursor = new Vector2 (localCursor.x + d.x/2, localCursor.y + d.y/2);
		// make value normalized
		localCursor = new Vector2(localCursor.x / d.x, localCursor.y / d.y);

		newHolePosition = localCursor;
	}

	public void OnRenderObject()
	{
		if (firstFrame)
		{
			firstFrame = false;
			RenderTexture old = RenderTexture.active;
			RenderTexture.active = renderTexture;
				GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 1 - targetImage.color.a));
			RenderTexture.active = old;
		}
		if (newHolePosition != null) {
			CutHole (newHolePosition.Value);
			newHolePosition = null;
		}
	}

	void OnGUI(){
		if(GUI.Button(new Rect(10f, 10f, 120f, 80f), "Restart"))
		{
			Application.LoadLevel(0);
		}
	}
}
