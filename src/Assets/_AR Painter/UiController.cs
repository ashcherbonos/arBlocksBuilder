using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UiController : MonoBehaviour {

	public static UiController instance;

	public GameObject StartCube;
	public List<Image> imagesToColor;
	public static Color32 color = Color.white;
	public GameObject colorPicker;
	public VerticalLayoutGroup layoutGroup;
	public Toggle defaultToggle;
	public Camera cameraForScreenshots;
	public GridLayoutGroup saveIconsGrid;
	public EasyTween saveIconsTween;
	public EasyTween toolPanelTween;
	public Slot slotPrefub;
	public GameObject[] buySlotButons;

	void Awake(){
		instance = this;
	}

	void Start(){
		
		//PlayerPrefs.DeleteAll ();
		BuildSaveIconsGrid ();

		DeviceChange.OnOrientationChange += OrientationChanged;
	}

	void OrientationChanged(DeviceOrientation orientation) {
		//Debug.Log ("OrientationChanged");
		//saveIconsGrid.gameObject.SetActive (false);
		BuildSaveIconsGrid ();
		//saveIconsGrid.gameObject.SetActive (true);
	}


	public void BuildSaveIconsGrid(){

		foreach (Transform child in saveIconsGrid.transform) {
			Destroy (child.gameObject);
		}

		for (int i = 0; i < Shop.slotsAmount; i++) {
			Slot slot = (Slot)Instantiate (slotPrefub);
			slot.transform.SetParent (saveIconsGrid.transform);
			slot.transform.localScale = Vector3.one;
			slot.id = i;
			slot.gameObject.SetActive (true);
		}
		float zoom =  GetComponent<CanvasScaler> ().referenceResolution.y / Screen.height;

		float cellSize = Screen.width>Screen.height ? (Screen.width/4) : (Screen.width/2);
		cellSize *= zoom;
		cellSize -= (saveIconsGrid.padding.left + saveIconsGrid.padding.right) / (Screen.width > Screen.height ? 4 : 2);
		cellSize = Mathf.Round (cellSize) - 1;
		saveIconsGrid.cellSize = new Vector2 (cellSize, cellSize);

		foreach (GameObject btnPrefub in buySlotButons) {
			GameObject btn = Instantiate (btnPrefub);
			btn.transform.SetParent (saveIconsGrid.transform);
			btn.transform.localScale = Vector3.one;
		}
	}

	public void SetColor(Color color){
		UiController.color = color;
		imagesToColor.ForEach(s => s.color = color);
	}

	public void ButtonZoomIn(){
		BuildBlock.zoom *= 1.1f;
	}
	public void ButtonZoomOut(){
		BuildBlock.zoom *= 0.9f;
	}
		

	public void ToggleColorPalete(bool newValue){
		//colorPicker.SetActive (newValue);
	}
	public void CloseColorPicker(){
		//colorPicker.SetActive (false);
	}

	public void TogglePlus(bool newValue){
		BuildBlock.addBlock = newValue;
	}

	public void ToggleMinus(bool newValue){
		BuildBlock.delBlock = newValue;
	}

	public void ToggleColorProbe(bool newValue){
		BuildBlock.colorZond = newValue;
	}

	public void ToggleSave(bool newValue){
		if (newValue) {
			BuildBlock.instance.Save ();
			saveIconsGrid.gameObject.SetActive (true);
			BuildSaveIconsGrid ();
			toolPanelTween.OpenCloseObjectAnimationTriggered (false);
			saveIconsTween.OpenCloseObjectAnimationTriggered (true);
		}

	}
	public void ButtonLoad(int id){
		toolPanelTween.OpenCloseObjectAnimationTriggered (true);
		saveIconsTween.OpenCloseObjectAnimationTriggered (false);
		BuildBlock.instance.Load (id);
		defaultToggle.isOn = true;
	}
		
	public void TogglePlace(bool newValue){
		//placeIcon.SetActive (newValue);
		//StartCube.SetActive (!newValue);
		//ARHitGO.SetActive (newValue);
		BuildBlock.placing = newValue;
		//BuildBlock.instance.rigidBody.isKinematic = !newValue;
		//BuildBlock.instance.parentCollider.enabled = newValue;
	}
		


	private void UnselectAllBtns(){
		BuildBlock.colorZond = false;
		BuildBlock.addBlock = false;
		BuildBlock.delBlock = false;
	}

	public void ButtonMoreGames(){
		#if UNITY_ANDROID
		Application.OpenURL("market://search?q=pub:Captain Flint");
		#elif UNITY_IOS
		Application.OpenURL("https://itunes.apple.com/us/developer/oleksandr-shcherbonos/id1040145110");
		#endif
	}
}
