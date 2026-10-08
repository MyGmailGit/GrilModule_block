using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Watermelon
{
    public class BlockMovementManager
    {
        private readonly static int GROUND_LAYER_MASK = 1 << PhysicsHelper.LAYER_GROUND;

        private const float MOVEMENT_SNAP_DISTANCE = 0.2f;
        private const float MOVEMENT_SPEED = 30;
        private const float MOVEMENT_STEP_SIZE = 0.1f;
        // private const Ease MOVEMENT_PREDICTION_EASING = Ease.Linear;

        private LevelRepresentation levelRepresentation;

        public LevelBlockBehavior BlockBehavior { get; private set; }

        private Vector3 touchOffset;

        private Vector3 lastAllowedPosition;
        private Vector3 targetPosition;

        private MovementMatrix movementMatrix;
        public MovementMatrix MovementMatrix => movementMatrix;

        private bool isBlockPicked;
        public bool IsBlockPicked => isBlockPicked;

        private List<LinkedObjectData> linkedObjects = new List<LinkedObjectData>();
        public List<LinkedObjectData> LinkedObjects => linkedObjects;

        // private readonly Ease.IEasingFunction easingFunction;

        public BlockMovementManager()
        {
            // easingFunction = Ease.GetFunction(MOVEMENT_PREDICTION_EASING);
        }

        public void SetLevelRepresentation(LevelRepresentation levelRepresentation)
        {
            this.levelRepresentation = levelRepresentation;
        }

        public void FixedUpdate()
        {
            if (!isBlockPicked)
                return;

            Ray ray = Camera.main.ScreenPointToRay(InputController.MousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, GROUND_LAYER_MASK))
            {
                MoveToPosition(hit.point);
            }
        }

        public void PickObject(LevelBlockBehavior levelBlockBehavior)
        {
            if (isBlockPicked) return;

            Ray ray = Camera.main.ScreenPointToRay(InputController.MousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, GROUND_LAYER_MASK))
            {
                isBlockPicked = true;

                touchOffset = hit.point - levelBlockBehavior.transform.position;
                touchOffset.y = 0;

                BlockBehavior = levelBlockBehavior;

                movementMatrix = new MovementMatrix(levelRepresentation, levelBlockBehavior);

                lastAllowedPosition = levelBlockBehavior.transform.position + touchOffset;
                targetPosition = lastAllowedPosition;

                movementMatrix.OverrideFigureStatus(levelBlockBehavior.Figure, levelBlockBehavior.MatrixPosition, MovementMatrix.STATUS_FREE);

                MoveToPosition(hit.point);
            }
        }

        public void Enable(LevelBlockBehavior levelBlockBehavior) // For Level Editor
        {
            if (isBlockPicked) return;

            isBlockPicked = true;
            touchOffset = Vector3.zero;
            BlockBehavior = levelBlockBehavior;
            movementMatrix = new MovementMatrix(levelRepresentation, levelBlockBehavior);
            lastAllowedPosition = levelBlockBehavior.transform.position;
            targetPosition = lastAllowedPosition;
            movementMatrix.OverrideFigureStatus(levelBlockBehavior.Figure, levelBlockBehavior.MatrixPosition, MovementMatrix.STATUS_FREE);
        }

        public void LinkObjects(List<LevelBlockBehavior> levelBlockBehaviors)
        {
            if (!isBlockPicked) return;

            foreach (LevelBlockBehavior block in levelBlockBehaviors)
            {
                if (block == BlockBehavior)
                    continue;

                Vector3 offset = block.transform.position - BlockBehavior.transform.position;
                offset.y = 0;

                linkedObjects.Add(new LinkedObjectData(block, offset));

                movementMatrix.OverrideFigureStatus(block.Figure, block.MatrixPosition, MovementMatrix.STATUS_FREE);
            }
        }

        public void SnapToClosestPosition()
        {
            if (!isBlockPicked) return;

            Vector2Int snapPosition = movementMatrix.WorldToGridPosition(BlockBehavior.transform.position);

            // I think there should not be this situation at all, but need to test
            if (snapPosition.x == -1 || snapPosition.y == -1 || !movementMatrix.IsMovementAllowed(BlockBehavior.Figure, new Vector3(snapPosition.x, 0f, snapPosition.y)))
            {
                Debug.LogWarning("Can't snap figure " + snapPosition + " " + BlockBehavior.transform.position);
            }
            else
            {
                BlockBehavior.transform.position = new Vector3(snapPosition.x, 0, snapPosition.y);

                foreach (LinkedObjectData linkedObject in linkedObjects)
                {
                    LevelBlockBehavior linkedBlock = linkedObject.Block;
                    linkedBlock.transform.position = BlockBehavior.transform.position + linkedObject.Offset;
                }
            }
        }

        public void ReleaseObject()
        {
            if (!isBlockPicked) return;

            movementMatrix.OverrideFigureStatus(BlockBehavior.Figure, BlockBehavior.MatrixPosition, MovementMatrix.STATUS_OCCUPIED);

            foreach (LinkedObjectData linkedObject in linkedObjects)
            {
                LevelBlockBehavior linkedBlock = linkedObject.Block;
                linkedBlock.transform.position = BlockBehavior.transform.position + linkedObject.Offset;

                movementMatrix.OverrideFigureStatus(linkedBlock.Figure, linkedBlock.MatrixPosition, MovementMatrix.STATUS_OCCUPIED);
            }

            linkedObjects.Clear();

            BlockBehavior = null;
            isBlockPicked = false;
        }

        public void MoveToPosition(Vector3 touchPosition)
        {
            if (!isBlockPicked) return;

            // Calculate the distance between the touch position and the last allowed position
            float totalDistance = Vector3.Distance(touchPosition, lastAllowedPosition);

            // Clamp the distance to optimize the number of steps
            float clampedDistance = Mathf.Clamp(totalDistance, 0, 1.5f);

            // Calculate the number of steps based on the distance and step size
            // With step size 0.1f we will have 15 steps for 1.5f distance
            int steps = Mathf.CeilToInt(clampedDistance / MOVEMENT_STEP_SIZE);

            bool horizontalMovementIsBlocked = false;
            bool verticalMovementIsBlocked = false;

            Vector3 extraOffset = Vector3.zero;

            for (int i = 0; i < steps; i++)
            {
                // Calculate the interpolation value for the easing function
                float p = (i + 1.0f) / steps;

                // Calculate the new position by moving a fixed step in the direction
                Vector3 lerpedPosition = Vector3.Lerp(lastAllowedPosition, touchPosition, p);//easingFunction.Interpolate(p));
                lerpedPosition.y = 0;

                if (movementMatrix.IsMovementAllowed(BlockBehavior.Figure, lerpedPosition - touchOffset, linkedObjects))
                {
                    if (!horizontalMovementIsBlocked && !verticalMovementIsBlocked)
                    {
                        lastAllowedPosition = lerpedPosition;
                        targetPosition = lastAllowedPosition - touchOffset;
                    }
                }
                else
                {
                    Vector3 horizontalPosition = new Vector3(lastAllowedPosition.x, 0, lerpedPosition.z);
                    if (movementMatrix.IsMovementAllowed(BlockBehavior.Figure, horizontalPosition - touchOffset, linkedObjects))
                    {
                        if (!verticalMovementIsBlocked)
                        {
                            lastAllowedPosition = horizontalPosition;
                            targetPosition = lastAllowedPosition - touchOffset;
                        }
                    }
                    else
                    {
                        verticalMovementIsBlocked = true;
                    }

                    Vector3 verticalPosition = new Vector3(lerpedPosition.x, 0, lastAllowedPosition.z);
                    if (movementMatrix.IsMovementAllowed(BlockBehavior.Figure, verticalPosition - touchOffset, linkedObjects))
                    {
                        if (!horizontalMovementIsBlocked)
                        {
                            lastAllowedPosition = verticalPosition;
                            targetPosition = lastAllowedPosition - touchOffset;
                        }
                    }
                    else
                    {
                        horizontalMovementIsBlocked = true;
                    }

                    if (verticalMovementIsBlocked && horizontalMovementIsBlocked)
                        break;

                }
            }

            Vector3 snapPosition = new Vector3(Mathf.RoundToInt(targetPosition.x), 0f, Mathf.RoundToInt(targetPosition.z));
            if (Vector3.Distance(targetPosition, snapPosition) < MOVEMENT_SNAP_DISTANCE)
            {
                BlockBehavior.transform.position = Vector3.Lerp(BlockBehavior.transform.position, snapPosition, Time.fixedDeltaTime * MOVEMENT_SPEED);
                lastAllowedPosition = BlockBehavior.transform.position + touchOffset;
            }
            else
            {
                BlockBehavior.transform.position = Vector3.Lerp(BlockBehavior.transform.position, targetPosition, Time.fixedDeltaTime * MOVEMENT_SPEED);
            }

            foreach (LinkedObjectData linkedObject in linkedObjects)
            {
                linkedObject.Block.transform.position = BlockBehavior.transform.position + linkedObject.Offset;
            }
        }

        public void DebugMatrix()
        {
            movementMatrix.DebugMatrix();
        }

        public class LinkedObjectData
        {
            public LevelBlockBehavior Block { get; }
            public Vector3 Offset { get; }

            public LinkedObjectData(LevelBlockBehavior block, Vector3 offset)
            {
                Block = block;
                Offset = offset;
            }
        }
    }
}
