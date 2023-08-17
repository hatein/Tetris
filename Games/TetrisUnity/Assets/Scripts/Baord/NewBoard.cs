﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace io.lockedroom.Games.TetrisUnity {

    public class NewBoard : MonoBehaviour {
        public Tilemap tilemap { get; private set; }
        public NewPiece activePiece { get; private set; }
        public TetrominoData[] tetrominoes;
        public Vector3Int spawnPosition;
        public Vector2Int boardSize = new Vector2Int(10, 20);
        
        public RectInt Bounds {
            get {
                Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
                return new RectInt(position, this.boardSize);
            }
        }
        private void Awake() {
            this.tilemap = GetComponentInChildren<Tilemap>();
            this.activePiece = GetComponentInChildren<NewPiece>();
            for (int i = 0; i < tetrominoes.Length; i++) {
                this.tetrominoes[i].Initialize();
            }
        }
        private void Start() {
            SpawnPiece();
        }
        public void SpawnPiece() {
            int random = Random.Range(0, this.tetrominoes.Length);
            TetrominoData data = this.tetrominoes[random];
            this.activePiece.Initialize(this, this.spawnPosition, data);
            if (IsValidPosition(this.activePiece, this.spawnPosition)) {
                Set(this.activePiece);
            }
            else {
                GameOver();
            }
            Set(this.activePiece);
        }
        private void GameOver() {
            this.tilemap.ClearAllTiles();
        }
        public void Set(NewPiece newpiece) {
            for (int i = 0; i < newpiece.cells.Length; i++) {
                Vector3Int tilePosition = newpiece.cells[i] + newpiece.position;
                this.tilemap.SetTile(tilePosition, newpiece.data.tile);
            }
        }
        public void Clear(NewPiece newpiece) {
            for (int i = 0; i < newpiece.cells.Length; i++) {
                Vector3Int tilePosition = newpiece.cells[i] + newpiece.position;
                this.tilemap.SetTile(tilePosition, null);
            }
        }
        public bool IsValidPosition(NewPiece newpiece, Vector3Int position) {
            RectInt bounds = this.Bounds;
            for (int i = 0; i < newpiece.cells.Length; i++) {
                Vector3Int tilePostion = newpiece.cells[i] + position;
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
                for (int col = bounds.xMin; col < bounds.xMax; col++) {
                    Vector3Int position = new Vector3Int(col, row + 1, 0);
                    TileBase above = this.tilemap.GetTile(position);
                    position = new Vector3Int(col, row, 0);
                    tilemap.SetTile(position, above);
                }
                row++;
            }
        }
    }
}

