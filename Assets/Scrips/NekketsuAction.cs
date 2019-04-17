﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// キーを押すと、移動する（熱血風対応版）
public class NekketsuAction : MonoBehaviour
{
    public float speed = 1;         // スピード：Inspectorで指定
    public float jumppower = 0.1f;  // ジャンプ力：Inspectorで指定

    float vx = 0;
    bool leftFlag = false; // 左向きかどうか
    bool pushFlag = false; // ジャンプキーを押しっぱなしかどうか
    bool jumpFlag = false; // ジャンプして空中にいるか

    Vector3 pos;

    float X = 0;    //内部での横
    float Y = 0;    //内部での高さ
    float Z = 0;    //内部での奥行き

    float G = 0;    //内部での重力※未実装
    float Spd = 0;  //内部での初速※未実装

    void Start() { } // 最初に行う

    void Update()
    { // ずっと行う

        vx = 0;
        pos = transform.position;

        // もし、右キーが押されたら
        if (Input.GetKey("right") || Input.GetAxis("Horizontal") > 0)
        {
            vx = speed; // 右に進む移動量を入れる
            leftFlag = false;

            X += vx;
        }
        // もし、左キーが押されたら
        if (Input.GetKey("left") || Input.GetAxis("Horizontal") < 0)
        {
            vx = -speed; // 左に進む移動量を入れる
            leftFlag = true;

            X += vx;
        }
        // もし、上キーが押されたら
        if (Input.GetKey("up") || Input.GetAxis("Vertical") > 0)
        {
            vx = speed * 0.4f; // 上に進む移動量を入れる(熱血っぽく奥行きは移動量小)

            Z += vx;
        }
        if (Input.GetKey("down") || Input.GetAxis("Vertical") < 0)
        { // もし、下キーが押されたら
            vx = speed * 0.4f; // 下に進む移動量を入れる(熱血っぽく奥行きは移動量小)

            Z += -vx;
        }

        // もし、ジャンプキーが押されたとき
        if (!(jumppower * 25 <= Y) && (Input.GetKey("a") || Input.GetKey("joystick button 2")))
        {
            // 着地済みかつ、ジャンプキー押しっぱなしでなければ
            if (Y <= 0 && pushFlag == false)
            { 
                jumpFlag = true; // ジャンプの準備
                pushFlag = true; // 押しっぱなし状態
            }
        }
        else
        {
            pushFlag = false; // 押しっぱなし解除
        }

        // ジャンプ上昇中状態
        if (jumpFlag && pushFlag)
        {
            vx = jumppower; // ジャンプの移動量を入れる

            // ジャンプが頂点に達しているか
            if (Y < jumppower * 25)
            {
                Y += vx;

                // 決められたジャンプの頂点より高く飛ばないように
                if (jumppower * 25 < Y)
                {
                    pushFlag = false;
                }
            }            
        }

        // ジャンプ下降中状態
        if (jumpFlag && !pushFlag)
        {
            vx = jumppower; // ジャンプの移動量を入れる

            // ジャンプ中なら下降させる。
            if (0 <= Y)
            {
                Y -= vx;
            }

            // 着地判定
            if (Y <= 0)
            {
                jumpFlag = false;
            }
        }



        // スプライト反転
        Vector3 scale = transform.localScale;
        if (leftFlag)
        {
            scale.x = -1; // 反転する（左向き）
        }
        else
        {
            scale.x = 1; // そのまま（右向き）
        }

        // 入力された内部XYZをtransformに設定する。
        pos.x = X;

        if (jumpFlag)
        {
            //ジャンプ中の場合は内部Yを加える。
            pos.y = Z + Y;
        }
        else
        {
            pos.y = Z;
        }

        transform.position = pos;
        transform.localScale = scale;

        // キャラクターの影の位置を設定。
        var shadeTransform = GameObject.Find("shade").transform;
        pos.y = Z - 0.8f;

        shadeTransform.position = pos;
    }

    void FixedUpdate() { } // ずっと行う（一定時間ごとに）
}