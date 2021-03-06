﻿using System.Collections;
using System.Collections.Generic;
using TsiU;
using UnityEngine;
using DG.Tweening;

namespace LinHowBehaviorTree
{
    /// <summary>
    /// 僵尸AI
    /// </summary>
    public class ZombieAI : UnityComponentSingleton<ZombieAI>
    {
        private TBTAction _behaviorTree;
        private AIEntityWorkingData _behaviorWorkingData;

        private void Awake()
        {
            _behaviorWorkingData = new AIEntityWorkingData();
            _behaviorWorkingData.entityAnimator = GetComponent<Animator>();
            _behaviorWorkingData.entityTF = transform;

           _behaviorTree = new TBTActionPrioritizedSelector();
           _behaviorTree
                .AddChild(new TBTActionSequence()
                    .SetPrecondition(new TBTPreconditionNOT(new HasReachedTarget()))
                    .AddChild(new TurnToAction())
                    .AddChild(new MoveToAction()))
               .AddChild(new TBTActionParallel()
                    .AddChild(new TurnToAction())   
                    .AddChild(new AttackAction()));

           
        }
        private void Update()
        {
            if (_behaviorTree.Evaluate(_behaviorWorkingData))
            {
                _behaviorTree.Update(_behaviorWorkingData);
            }
            else
            {
                _behaviorTree.Transition(_behaviorWorkingData);
            }
        }
        class HasReachedTarget : TBTPreconditionLeaf
        {
            public override bool IsTrue(TBTWorkingData wData)
            {
                AIEntityWorkingData thisData = wData.As<AIEntityWorkingData>();

                return Vector3.Distance(
                    thisData.entityTF.position,
                    WarriorAI.Instance.transform.position) < 1.5f;

            }
        }

        class TurnToAction: TBTActionLeaf
        {
            private int TurnToStatus = TBTRunningStatus.EXECUTING;
            
            protected override void onEnter(TBTWorkingData wData)
            {
                TurnToStatus = TBTRunningStatus.EXECUTING;
                AIEntityWorkingData thisData = wData.As<AIEntityWorkingData>();
                thisData.entityTF.DOLookAt(WarriorAI.Instance.transform.position, 1.5f)
                                       .OnComplete(() => UpdateStatus());
            }
            protected override bool onEvaluate(TBTWorkingData wData)
            {
                return base.onEvaluate(wData);
            }
            protected override int onExecute(TBTWorkingData wData)
            {
                return TurnToStatus;
            }
            protected override void onExit(TBTWorkingData wData, int runningStatus)
            {
                base.onExit(wData, runningStatus);
            }
            private void UpdateStatus()
            {
                TurnToStatus = TBTRunningStatus.FINISHED;
            }
        }

        class MoveToAction:TBTActionLeaf
        {
            private int MoveToStatus = TBTRunningStatus.EXECUTING;
            protected override void onEnter(TBTWorkingData wData)
            {
                AIEntityWorkingData thisData = wData.As<AIEntityWorkingData>();
                thisData.entityAnimator.Play("walk");
                MoveToStatus = TBTRunningStatus.EXECUTING;
                

            }
            protected override bool onEvaluate(TBTWorkingData wData)
            {

                return base.onEvaluate(wData);
            }
            protected override int onExecute(TBTWorkingData wData)
            {
                AIEntityWorkingData thisData = wData.As<AIEntityWorkingData>();
                Vector3 moveDistance = Vector3.Normalize(WarriorAI.Instance.transform.position - thisData.entityTF.position)*0.1f;

                thisData.entityTF.DOMove(thisData.entityTF.position + moveDistance, Time.deltaTime)
                    .OnComplete(() => UpdateStatus());                
                return MoveToStatus;

            }
            protected override void onExit(TBTWorkingData wData, int runningStatus)
            {
                base.onExit(wData, runningStatus);
            }
            private void UpdateStatus()
            {
                MoveToStatus = TBTRunningStatus.FINISHED;
            }
        }

        class AttackAction:TBTActionLeaf
        {
            protected override void onEnter(TBTWorkingData wData)
            {
                AIEntityWorkingData thisData = wData.As<AIEntityWorkingData>();
                thisData.entityAnimator.CrossFade("attack", 0.2f);
            }
            protected override int onExecute(TBTWorkingData wData)
            {
                return TBTRunningStatus.EXECUTING;
            }
            protected override bool onEvaluate(TBTWorkingData wData)
            { 
                return base.onEvaluate(wData);
            }
            protected override void onExit(TBTWorkingData wData, int runningStatus)
            {
            }
        }
    }
}

