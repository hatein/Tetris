using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
namespace io.lockedroom.Games.TetrisUnity {

    public class NewPiece : MonoBehaviour {
        public NewBoard newboard { get; private set; }
        public TetrominoData data { get; private set; }
        public Vector3Int[] cells { get; private set; }
        public Vector3Int position { get; private set; }
        public int rotationIndex { get; private set; }
        public float setpDelay = 1f;
        public float lockDelay = 0.5f;
        private float stepTime;
        private float lockTime;
        public void Initialize(NewBoard newboard, Vector3Int position, TetrominoData data) {
            this.newboard = newboard;
            this.position = position;
            this.data = data;
            rotationIndex = 0;
            this.stepTime = Time.time + this.setpDelay;
            this.lockTime = 0f;
            if (this.cells == null) {
                this.cells = new Vector3Int[data.cells.Length];
            }
            for (int i = 0; i < data.cells.Length; i++) {
                this.cells[i] = (Vector3Int)data.cells[i];
            }
        }
        private void Update() {
            this.newboard.Clear(this);
            this.lockTime += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.U)) {
                Rotate(-1);
            }
            else if (Input.GetKeyDown(KeyCode.O)) {
                Rotate(1);
            }
            if (Input.GetKeyDown(KeyCode.J)) {
                Move(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.L)) {
                Move(Vector2Int.right);
            }
            if (Input.GetKeyDown(KeyCode.K)) {
                if (Move(Vector2Int.down)) {

                }
            }
            if (Input.GetKeyDown(KeyCode.I)) {
                HardDrop();
            }
            if (Time.time > this.stepTime) {
                Step();
            }
            this.newboard.Set(this);
        }
        private void Step() {
            this.stepTime = Time.time + this.setpDelay;
            Move(Vector2Int.down);
            if (this.lockTime >= this.lockDelay) {
                Lock();
            }
        }
        private void Lock() {
            this.newboard.Set(this);
            this.newboard.SpawnPiece();
            this.newboard.ClearLines();
        }
        private void HardDrop() {
            while (Move(Vector2Int.down)) { continue; }
            Lock();
        }

        private bool Move(Vector2Int translation) {
            Vector3Int newPosition = this.position;
            newPosition.x += translation.x;
            newPosition.y += translation.y;
            bool valid = this.newboard.IsValidPosition(this, newPosition);
            if (valid) {
                this.position = newPosition;
                this.lockTime = 0f;
            }
            return valid;
        }
        private void Rotate(int direction) {
            int originalRotation = this.rotationIndex;
            this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);
            ApplyRotationMatrix(direction);
            if (!TestWallKicks(this.rotationIndex, direction)) {
                this.rotationIndex = originalRotation;
                ApplyRotationMatrix(-direction);
            }
        }
        private void ApplyRotationMatrix(int direction) {
            for (int i = 0; i < this.cells.Length; i++) {
                Vector3 cell = this.cells[i];
                int x, y;
                switch (this.data.tetromino) {
                    case Tetromino.I:
                    case Tetromino.O:
                        // "I" and "O" are rotated from an offset center point
                        cell.x -= 0.5f;
                        cell.y -= 0.5f;
                        x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                        y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                        break;

                    default:
                        x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                        y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                        break;
                }

                cells[i] = new Vector3Int(x, y, 0);
            }
        }
        private bool TestWallKicks(int rotationIndex, int rotationDirection) {
            int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

            for (int i = 0; i < data.wallKicks.GetLength(1); i++) {
                Vector2Int translation = data.wallKicks[wallKickIndex, i];

                if (Move(translation)) {
                    return true;
                }
            }

            return false;
        }

        private int GetWallKickIndex(int rotationIndex, int rotationDirection) {
            int wallKickIndex = rotationIndex * 2;

            if (rotationDirection < 0) {
                wallKickIndex--;
            }

            return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
        }

        private int Wrap(int input, int min, int max) {
            if (input < min) {
                return max - (min - input) % (max - min);
            }
            else {
                return min + (input - min) % (max - min);
            }
        }
    }
}
