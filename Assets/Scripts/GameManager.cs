using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public Maze mazePrefab;

	public Player playerPrefab;

	public bool cameraFollowPlayer = true;
	public float cameraHeight = 15f;
	public float cameraDistance = 10f;

	private Maze mazeInstance;

	private Player playerInstance;
	private Camera mainCamera;

	private void Start () {
		StartCoroutine(BeginGame());
	}
	
	private void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			RestartGame();
		}
	}

	private void LateUpdate () {
		if (cameraFollowPlayer && playerInstance != null && mainCamera != null) {
			Vector3 playerPos = playerInstance.transform.position;
			mainCamera.transform.position = new Vector3(playerPos.x, playerPos.y + cameraHeight, playerPos.z - cameraDistance);
			mainCamera.transform.LookAt(playerPos);
		}
	}

	private IEnumerator BeginGame () {
		mainCamera = Camera.main;
		if (mainCamera == null) {
			Debug.LogError("No main camera found! Please ensure there is a camera tagged as 'MainCamera' in the scene.");
			yield break;
		}
		
		mainCamera.clearFlags = CameraClearFlags.Skybox;
		mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
		mazeInstance = Instantiate(mazePrefab) as Maze;
		yield return StartCoroutine(mazeInstance.Generate());
		playerInstance = Instantiate(playerPrefab) as Player;
		//playerInstance.TeleportToCell(
		//	mazeInstance.GetCell(mazeInstance.RandomCoordinates));
		playerInstance.TeleportToCell(
			mazeInstance.GetCell(mazeInstance.CenterCoordinates));
		
		// Position camera above and behind the player to see the maze
		Vector3 playerPos = playerInstance.transform.position;
		mainCamera.transform.position = new Vector3(playerPos.x, playerPos.y + cameraHeight, playerPos.z - cameraDistance);
		mainCamera.transform.LookAt(playerPos);
		mainCamera.clearFlags = CameraClearFlags.SolidColor;
		mainCamera.backgroundColor = new Color(0.2f, 0.2f, 0.3f, 1f);
		mainCamera.rect = new Rect(0f, 0f, 1f, 1f); // Full screen view
		
		Color ambientColor = new Color();
		ambientColor.r = 0.5f;
		ambientColor.g = 0.5f;
		ambientColor.b = 0.5f;
		RenderSettings.ambientLight = ambientColor;
	}

	private void RestartGame () {
		StopAllCoroutines();
		Destroy(mazeInstance.gameObject);
		if (playerInstance != null) {
			Destroy(playerInstance.gameObject);
		}
		StartCoroutine(BeginGame());
	}
}