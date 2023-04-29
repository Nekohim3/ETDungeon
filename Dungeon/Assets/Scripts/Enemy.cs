using System;
using Assets._Scripts.AStar;
using Assets._Scripts.DungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Extensions;
using SkiaSharp;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float          MoveSpeed = 15;
    Rigidbody2D           _rb;
    private Transform     _target;
    private Vector2       _movement;
    private Vector3       _nextPoint;
    private bool          _needNextPoint = true;
    private AStar         _aStar         = new AStar(false);
    private List<Vector3> _currentPath;
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>(); 
        //var p = new AStar(false, 1000);
        //var lst = p.Calculate(new SKPointI(Mathf.RoundToInt(_rb.position.x),     Mathf.RoundToInt(-_rb.position.y)),
        //                      new SKPointI(Mathf.RoundToInt(_target.position.x), Mathf.RoundToInt(-_target.position.y)), CurrentMap.MapArr);
        //_nextPoint = new Vector3(lst[^2].X, -lst[^2].Y);
        //_movement  = (_nextPoint - transform.position).normalized;
    }

    void Start()
    {
        _target = GameObject.FindWithTag("Player").transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            
        }
    }

    void Update()
    {
        if (_target)
        {
            if (_needNextPoint || (Math.Abs(_nextPoint.x - _rb.position.x) > 1 || Math.Abs(_nextPoint.y - _rb.position.y) > 1))
            {
                _currentPath = _aStar.Calculate(new SKPointI(Mathf.RoundToInt(_rb.position.x),     Mathf.RoundToInt(-_rb.position.y)),
                                           new SKPointI(Mathf.RoundToInt(_target.position.x), Mathf.RoundToInt(-_target.position.y)), CurrentMap.MapArr).Select(_ => _.ToVector3().InvertY()).ToList();
                _currentPath.Reverse();
                var np = _currentPath[1];
                if (np.CloseToVector3(_rb.position))
                {

                }
                else
                {
                    _nextPoint     = np;
                    _movement      = (_nextPoint - transform.position).normalized;
                    _needNextPoint = false;
                }
            }
            //if (_nextPoint == null || _nextPoint.Count == 0)
            //{
            //    var lst = _aStar.Calculate(new SKPointI(Mathf.RoundToInt(_rb.position.x),     Mathf.RoundToInt(-_rb.position.y)),
            //                               new SKPointI(Mathf.RoundToInt(_target.position.x), Mathf.RoundToInt(-_target.position.y)), CurrentMap.MapArr);
            //    if (lst != null)
            //    {
            //        _nextPoint =  lst.ToVector3();

            //        _nextPoint.Reverse();
            //    }
            //}
            //else
            //{

            //    if (Math.Abs(_nextPoint[0].x - _rb.position.x) < 0.05f && Math.Abs(_nextPoint[0].y + _rb.position.y) < 0.05f)
            //    {
            //        _nextPoint.RemoveAt(0);
            //    }
            //    else
            //    {
            //        _movement = (new Vector3(_nextPoint[0].x, -_nextPoint[0].y) - transform.position).normalized;

            //    }
            //}
            //
            //{
                //var p = new AStar(false, 1000);
                //var lst = p.Calculate(new SKPointI(Mathf.RoundToInt(_rb.position.x),     Mathf.RoundToInt(-_rb.position.y)),
                                      //new SKPointI(Mathf.RoundToInt(_target.position.x), Mathf.RoundToInt(-_target.position.y)), CurrentMap.MapArr);
                
                //_nextPoint = new Vector3(lst[^2].X, -lst[^2].Y);
                //_movement  = (_nextPoint - transform.position).normalized;
                //}
        }
    }

    private void FixedUpdate()
    {
        if (_nextPoint.CloseToVector3(_rb.position))
        {
            _needNextPoint = true;
            _movement      = new Vector2(0, 0);
        }
        _rb.velocity = _movement * MoveSpeed;
    }
}
