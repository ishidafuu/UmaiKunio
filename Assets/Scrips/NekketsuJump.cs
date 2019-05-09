﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NTD = NekketsuTypeDefinition;

public class NekketsuJump
{
    // GameObject GObj; //ゲームオブジェクトそのものが入る変数
    NekketsuAction NAct; //NekketsuActionが入る変数

    bool miniJumpFlag = false; // 小ジャンプ
    bool jumpAccelerate = false;    //ジャンプ加速度の計算を行うフラグ
    NTD.VectorX JumpX = 0; // ジャンプの瞬間に入力していた疑似Xの方向(左、右、ニュートラル)
    NTD.VectorZ JumpZ = 0; // ジャンプの瞬間に入力していた疑似Zの方向(手前、奥、ニュートラル)
    bool leftJumpFlag = false; // ジャンプの瞬間に向いていた方向。左向きかどうか
    float nowTimesquat = 0f; //しゃがみ状態硬直時間を計測

    public NekketsuJump(NekketsuAction nekketsuAction)
    {
        NAct = nekketsuAction;
    }

    public void JumpMain()
    {
        #region 空中制御

        if (!NAct.squatFlag)
        {

            if (NAct.jumpFlag && !NAct.dashFlag)
            {
                // 空中制御 疑似X軸
                switch (JumpX)
                {
                    case NTD.VectorX.None:

                        if (JumpZ == NTD.VectorZ.None)
                        {
                            if (!leftJumpFlag)
                            {
                                // 右向き時の垂直ジャンプ中に右キーが押されたら
                                if (NAct.XInputState == NTD.XInputState.XRightPushMoment
                                    || NAct.XInputState == NTD.XInputState.XRightPushButton)
                                {
                                    NAct.vx = NAct.speed; // 右に進む移動量を入れる
                                    NAct.leftFlag = false;

                                    NAct.X += NAct.vx;
                                }
                            }
                            else
                            {
                                // 左向き時の垂直ジャンプ中に左キーが押されたら ★else if でもキーボード同時押し対策NG★
                                if (NAct.XInputState == NTD.XInputState.XLeftPushMoment
                                    || NAct.XInputState == NTD.XInputState.XLeftPushButton)
                                {
                                    NAct.vx = -NAct.speed; // 左に進む移動量を入れる
                                    NAct.leftFlag = true;

                                    NAct.X += NAct.vx;
                                }
                            }
                        }
                        break;

                    case NTD.VectorX.Right:
                        NAct.vx = NAct.speed; // 右に進む移動量を入れる
                        NAct.X += NAct.vx;

                        // もし、右空中移動中に、左キーが押されたら  ★else if でもキーボード同時押し対策NG★
                        if (NAct.XInputState == NTD.XInputState.XLeftPushMoment
                            || NAct.XInputState == NTD.XInputState.XLeftPushButton)
                        {
                            NAct.vx = -NAct.speed; // 左に進む移動量を入れる
                            NAct.leftFlag = true;

                            NAct.X += NAct.vx * 0.2f;
                        }

                        break;


                    case NTD.VectorX.Left:
                        NAct.vx = -NAct.speed; // 左に進む移動量を入れる
                        NAct.X += NAct.vx;

                        // もし、左空中移動中に、右キーが押されたら
                        if (NAct.XInputState == NTD.XInputState.XRightPushMoment
                            || NAct.XInputState == NTD.XInputState.XRightPushButton)
                        {
                            NAct.vx = NAct.speed; // 右に進む移動量を入れる
                            NAct.leftFlag = false;

                            NAct.X += NAct.vx * 0.2f;
                        }
                        break;
                }

                // 空中制御 疑似Z軸
                switch (JumpZ)
                {
                    case NTD.VectorZ.None:
                        //FC・再っぽく、垂直ジャンプからのZ入力は受け付けない　とりあえず。
                        break;

                    case NTD.VectorZ.Up:
                        NAct.vz = NAct.speed * 0.4f; // 上に進む移動量を入れる(熱血っぽく奥行きは移動量小)
                        NAct.Z += NAct.vz;
                        break;

                    case NTD.VectorZ.Down:
                        NAct.vz = -NAct.speed * 0.4f; // 下に進む移動量を入れる(熱血っぽく奥行きは移動量小)
                        NAct.Z += NAct.vz;
                        break;
                }
            }
        }

        #endregion

        #region ジャンプ処理

        if (!NAct.squatFlag && !NAct.brakeFlag)
        {
            // ジャンプした瞬間
            if (!NAct.jumpFlag
                && NAct.JumpButtonState == NTD.JumpButtonPushState.PushMoment)
            {
                // 着地状態
                if (NAct.Y <= 0)
                {
                    NAct.jumpFlag = true; // ジャンプの準備
                    miniJumpFlag = false; // 小ジャンプ
                    NAct.vy += NAct.InitalVelocity; // ジャンプした瞬間に初速を追加
                    jumpAccelerate = true; // ジャンプ加速度の計算を行う

                    // 空中制御用に、ジャンプした瞬間に入力していたキーを覚えておく(X軸)
                    if (NAct.XInputState == NTD.XInputState.XRightPushMoment
                        || NAct.XInputState == NTD.XInputState.XRightPushButton)
                    {
                        JumpX = NTD.VectorX.Right;
                    }
                    else if (NAct.XInputState == NTD.XInputState.XLeftPushMoment
                            || NAct.XInputState == NTD.XInputState.XLeftPushButton)
                    {
                        JumpX = NTD.VectorX.Left;
                    }
                    else
                    {
                        // 垂直ジャンプであれば、向いていた方向を覚えておく。
                        JumpX = NTD.VectorX.None;
                        leftJumpFlag = NAct.leftFlag;
                    }

                    // 空中制御用に、ジャンプした瞬間に入力していたキーを覚えておく(Z軸)
                    if (NAct.ZInputState == NTD.ZInputState.ZBackPushMoment
                        || NAct.ZInputState == NTD.ZInputState.ZBackPushButton)
                    {
                        JumpZ = NTD.VectorZ.Up;
                    }
                    else if (NAct.ZInputState == NTD.ZInputState.ZFrontPushMoment
                            || NAct.ZInputState == NTD.ZInputState.ZFrontPushButton)
                    {
                        JumpZ = NTD.VectorZ.Down;
                    }
                    else
                    {
                        JumpZ = NTD.VectorZ.None;
                    }
                }
            }

            // ジャンプ状態
            if (NAct.jumpFlag)
            {
                // ジャンプボタンが離されたかつ、上昇中かつ、小ジャンプフラグが立ってない
                // 小ジャンプ処理
                if (NAct.JumpButtonState == NTD.JumpButtonPushState.ReleaseButton 
                    && NAct.vy > 0 
                    && !miniJumpFlag)
                {
                    // 小ジャンプ用に、現在の上昇速度を半分にする
                    NAct.vy = NAct.vy * 0.5f;

                    // 小ジャンプフラグTrue
                    miniJumpFlag = true;
                }

                // ジャンプ中の重力加算(重力は変化せず常に同じ値が掛かる)
                NAct.vy += NAct.Gravity;

                NAct.Y += NAct.vy; // ジャンプ力を加算

                // 着地判定
                if (NAct.Y <= 0)
                {
                    NAct.jumpFlag = false;

                    // ★ジャンプボタン押しっぱなし対策必要★
                    NAct.JumpButtonState = NTD.JumpButtonPushState.None;
                    // ★ジャンプボタン押しっぱなし対策必要★

                    NAct.dashFlag = false; //ダッシュ中であれば、着地時にダッシュ解除

                    NAct.squatFlag = true; //しゃがみ状態

                    // 地面めりこみ補正は着地したタイミングで行う
                    if (NAct.Y < 0)
                    {
                        NAct.Y = 0; // マイナス値は入れないようにする
                    }

                    if (NAct.InitalVelocity != 0)
                    {
                        // 内部Y軸変数を初期値に戻す
                        NAct.vy = 0;
                    }
                }
            }

            // ジャンプ加速度の計算
            if (NAct.jumpFlag && jumpAccelerate)
            {
                // ジャンプ時、横移動の初速を考慮
                if (NAct.jumpFlag &&
                    (NAct.XInputState == NTD.XInputState.XRightPushMoment
                    || NAct.XInputState == NTD.XInputState.XRightPushButton)
                    || (NAct.XInputState == NTD.XInputState.XLeftPushMoment
                       || NAct.XInputState == NTD.XInputState.XLeftPushButton))
                {
                    if (NAct.leftFlag)
                    {
                        NAct.X += -(NAct.InitalVelocity * NAct.speed);
                    }
                    else
                    {
                        NAct.X += NAct.InitalVelocity * NAct.speed;
                    }
                }
                else
                {
                    jumpAccelerate = false;
                }
            }
        }

        // しゃがみ状態の処理
        if (NAct.squatFlag)
        {
            // しゃがみ状態の時間計測
            nowTimesquat += Time.deltaTime;

            // しゃがみ状態解除
            if (nowTimesquat > 0.1f)
            {
                NAct.squatFlag = false;
                nowTimesquat = 0;
            }
        }
        #endregion
    }
}
