using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class Slot : MonoBehaviour
{

	public int id;
	public Image image;
	// Use this for initialization
	void OnEnable ()
	{
		// loading icon:
		if (File.Exists (SaveController.ScreenFileName (id))) {
			
			Texture2D tex = new Texture2D (SaveController.iconWidth, SaveController.iconHeight, TextureFormat.RGB24, false);
			if (tex.LoadImage (File.ReadAllBytes (SaveController.ScreenFileName (id)))) {
				tex.Apply (false, true);
				image.sprite = Sprite.Create (tex, new Rect (0, 0, SaveController.iconWidth, SaveController.iconHeight), new Vector2 (0f, 1f), 1);
			}
		}
	}

	public void OnMouseDown(){
		UiController.instance.ButtonLoad (id);
	}
}