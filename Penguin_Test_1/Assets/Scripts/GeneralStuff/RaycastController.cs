﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    protected const float skinWidth = .015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;
    protected BoxCollider2D collider;
    protected RaycastOrigins raycastOrigins;

    public LayerMask collisionMask;

    protected virtual void Awake()
    {
        collider = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();
    }

    protected virtual void Start()
    {
        CalculateRaySpacing();
    }

    //Обновляем крайние точки для хитбокса
    protected void UpdateRaycastOrigins()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();

        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }


    //Вычисляем расстояние между лучами
    protected void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    protected struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
