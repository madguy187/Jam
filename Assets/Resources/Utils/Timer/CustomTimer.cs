using System;
using UnityEngine;

public class CustomTimer {
    float _fCurrentTimer = 0.0f;

    float _fTimeToAct = 0.0f;
    public void SetTime(float time) { _fTimeToAct = time; }

    Action _func;
    public void SetFunc(Action func) { _func = func; }

    bool _bActive = false;

    // Update is called once per frame
    public void Update() {
        if (!_bActive) {
            return;
        }
        
        if (_fCurrentTimer < _fTimeToAct) {
            _fCurrentTimer += Time.deltaTime;
            return;
        }


        if (_func != null) {
            _func();
        }
        _fCurrentTimer = 0.0f;
        _bActive = false;
    }

    public void Reset() {
        _bActive = true;
        _fCurrentTimer = 0.0f;
    }
}
