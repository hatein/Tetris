using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace io.lockedroom.Games.TetrisUnity {

    public class Board : MonoBehaviour {
        [SerializeField] private AudioSource clear;
        public Tilemap tilemap { get; private set; }
        public Piece activePiece { get; private set; }
        public Piece nextPiece { get; private set; } 
        public Piece savedPiece { get; private set; }
        
        public int scoreOneLine = 40;
        public int scoreTwoLine = 100;
        public int scoreThreeLine = 120;
        public int scoreFourLine = 200;
        public Text hud_score;
        private int numberOfrowsthisTurn = 0;
        private int currentscore = 0;


        public TetrominoData[] tetrominoes;
        public Vector3Int spawnPosition;
        public Vector2Int boardSize = new Vector2Int(10, 20);
        public Vector3Int previewPosition = new Vector3Int(-1, 12, 0);
        public Vector3Int holdPosition = new Vector3Int(-1, 16, 0);
        
        public RectInt Bounds {
            get {
                Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
                return new RectInt(position, this.boardSize);
            }
        }
        private void Awake() {
            this.tilemap = GetComponentInChildren<Tilemap>();
            this.activePiece = GetComponentInChildren<Piece>();
            nextPiece = gameObject.AddComponent<Piece>();
            nextPiece.enabled = false;
            savedPiece = gameObject.AddComponent<Piece>();
            savedPiece.enabled = false;
            for (int i = 0; i < tetrominoes.Length; i++) {
                this.tetrominoes[i].Initialize();
            }
            ;
        }
        private void Start() {
            SetNextPiece();
            SpawnPiece();
        }

        private void Update() {
            UpdateScore();
            UpdateUI();
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) {
                SwapPiece();
            }
        }
        
        public void UpdateUI() {
            hud_score.text = currentscore.ToString();
        }
        public void UpdateScore() {
           if(numberOfrowsthisTurn > 0) {
                if(numberOfrowsthisTurn == 1) {
                    ClearedOneLine();
                }
                else if (numberOfrowsthisTurn == 2) {
                    ClearedTwoLine();
                }
                else if (numberOfrowsthisTurn == 3) {
                    ClearedThreeLine();
                }
                else if (numberOfrowsthisTurn == 4) {
                    ClearedFourLine();
                }
                numberOfrowsthisTurn = 0;
            }
        }
        public void ClearedOneLine() {
            currentscore += scoreOneLine;
        }
        public void ClearedTwoLine() {
            currentscore += scoreTwoLine;
        }
        public void ClearedThreeLine() {
            currentscore += scoreThreeLine;
        }
        public void ClearedFourLine() {
            currentscore += scoreFourLine;
        }

        public void SpawnPiece() {
            this.activePiece.Initialize(this, this.spawnPosition, nextPiece.data);
            if (IsValidPosition(this.activePiece,this.spawnPosition)) {
                Set(this.activePiece);
            }
            else {
                GameOver();
            }
            SetNextPiece();
        }
        private void GameOver() {
            Application.LoadLevel("Gameover");
        }

        public void Set(Piece piece) {
            for (int i = 0; i < piece.cells.Length; i++) {
                Vector3Int tilePosition = piece.cells[i] + piece.position;
                this.tilemap.SetTile(tilePosition, piece.data.tile);
            }
        }
        public void Clear(Piece piece) {
            for (int i = 0; i < piece.cells.Length; i++) {
                Vector3Int tilePosition = piece.cells[i] + piece.position;
                this.tilemap.SetTile(tilePosition, null);
            }
        }
        public bool IsValidPosition(Piece piece, Vector3Int position) {
            RectInt bounds = this.Bounds;
            for (int i = 0; i < piece.cells.Length; i++) {
                Vector3Int tilePostion = piece.cells[i] + position;
                if (!bounds.Contains((Vector2Int)tilePostion)) {
                    return false;
                }
                if (this.tilemap.HasTile(tilePostion)) {
                    return false;
                }
            }
            return true;
        }
        public void ClearLines() {
            RectInt bounds = this.Bounds;
            int row = bounds.yMin;
            while (row < bounds.yMax) {
                if (IsLineFull(row)) {
                    LineClear(row);
                }
                else { row++; }
            }

        }
        private bool IsLineFull(int row) {
            RectInt bounds = this.Bounds;
            for (int col = bounds.xMin; col < bounds.xMax; col++) {
                Vector3Int position = new Vector3Int(col, row, 0);
                if (!this.tilemap.HasTile(position)) { return false; }
            }
            return true;
        }
        private void LineClear(int row) {
            RectInt bounds = this.Bounds;
            for (int col = bounds.xMin; col < bounds.xMax; col++) {
                Vector3Int position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, null);
            }
            while (row < bounds.xMax) {
                for(int col = bounds.xMin; col < bounds.xMax; col++) {
                    Vector3Int position = new Vector3Int(col, row+1 , 0);
                    TileBase above = this.tilemap.GetTile(position); 
                    position = new Vector3Int(col,row,0);
                    tilemap.SetTile(position, above);
                }
                row++;
            }
            clear.Play();
            numberOfrowsthisTurn++;
        }
        private void SetNextPiece() {
            if (nextPiece.cells != null) {
                Clear(nextPiece);
            }
            int random = Random.Range(0, tetrominoes.Length);
            TetrominoData data = tetrominoes[random];
            nextPiece.Initialize(this, previewPosition, data);
            Set(nextPiece);
        }
        public void SwapPiece() {
            TetrominoData savedData = savedPiece.data;
            if (savedData.cells != null) {
                Clear(savedPiece);
            }
            savedPiece.Initialize(this, holdPosition, nextPiece.data);
            Set(savedPiece);
                if (savedData.cells != null) {
                Clear(nextPiece);
                nextPiece.Initialize(this, previewPosition, savedData);
                Set(nextPiece);
            }
        }
    } 
}

