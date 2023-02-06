using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    [SerializeField]
    SEController mSEController;
    [SerializeField]
    BGMController mBGMController;

    #region override
    protected override void Awake()
    {
        base.Awake();
        mSEController.Init();
        mBGMController.Init();
    }
    #endregion // override

    public void SEPlay(string name, Vector3 pos, float _volume = 1.0f)
    {
        mSEController.Play(name, pos, _volume);
    }

    public void BGMPlay(string name, Vector3 pos, float _volume)
    {
        mBGMController.Play(name, pos, _volume);
    }

    public void SetSEVolume(float _volume)
    {
        mSEController.setVolume(_volume);
    }
    public void SetBGMVolume(float _volume)
    {
        mSEController.setVolume(_volume);
    }

    public void FadeOutSE(string _name, float _fadeTime)
    {
        mSEController.Fade(_name, 0.0f, _fadeTime);
    }

    [Serializable]
    abstract class AudioClass
    {
        public struct ClipInfo
        {
            public string name;
            public AudioClip clip;
        }

        float mVolumeRate = 1.0f;

        [SerializeField]
        List<AudioSource> mSourceList;
        [SerializeField]
        List<ClipInfo> mClipList;
        protected Dictionary<string, AudioClip> mClipDict = new Dictionary<string, AudioClip>();


        public void Init()
        {
            // リストをDictionary形式に変更
            foreach (var se in mClipList)
            {
                mClipDict.Add(se.name, se.clip);
            }
        }
        
        public void setVolume(float _rate)
        {
            mVolumeRate = _rate;
        }

        public void AllReset()
        {
            foreach (var source in mSourceList)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
            }
        }
        public void Fade(string _name, float _volume, float _fadeTime)
        {
            foreach (var source in mSourceList)
            {
                if (source.isPlaying && source.clip == mClipDict[_name])
                {
                    SoundManager.Instance.StartCoroutine(Fade(source,_volume, _fadeTime));
                }
            }
        }

        public abstract bool Play(string name, Vector3 pos, float _volume = 1.0f);

        /// <summary>
        ///  使用されていないAudioSourceを探す
        /// </summary>
        protected List<AudioSource> SearchUnUseSourceList()
        {
            List<AudioSource> sourceList = new List<AudioSource>();
            foreach (var source in mSourceList)
            {
                if (source.isPlaying)
                {
                    sourceList.Add(source);
                }
            }
            return sourceList;
        }

        /// <summary>
        ///  使用されているAudioSourceを探す
        /// </summary>
        protected List<AudioSource> SearchUseSourceList()
        {
            List<AudioSource> sourceList = new List<AudioSource>();
            foreach (var source in mSourceList)
            {
                if (source.isPlaying)
                {
                    sourceList.Add(source);
                }
            }
            return sourceList;
        }

        /// <summary>
        /// 指定したAudioSourceの音をvolumeまでフェードする
        /// 音量が0まで行くとAudioSourceをとめて
        /// AudioSourceがなっていない場合は音を鳴らし始める
        /// </summary>
        protected IEnumerator Fade(AudioSource _source, float _volume, float _fadeTime)
        {
            if (_source.volume == _volume)
                yield break;

            if (!_source.isPlaying)
            {
                _source.volume = 0.0f;
                _source.Play();
            }

            bool isFadeUp = _volume > _source.volume; 
            float fadeSpeed = (_volume - _source.volume) / (_fadeTime / Time.deltaTime);

            while (true)
            {
                _source.volume += fadeSpeed;
                if((isFadeUp && _volume < _source.volume)
                    || (!isFadeUp && _volume > _source.volume)) 
                {
                    _source.volume = _volume;
                    break;
                }
                yield return null;
            }
            if(_source.volume == 0)
            {
                _source.Stop();
            }
            yield break;
        }
    }

    [Serializable]
    class SEController : AudioClass
    {
        public override bool Play(string name, Vector3 pos, float _volume = 1f)
        {
            // volumeを0から1の間になるようにする
            _volume = Mathf.Clamp(_volume, 0.0f, 1.0f);
            // 使っていないAudioSourceを探す
            AudioSource SeSource = SearchUnUseSourceList()[0];

            if (SeSource == null)
            {
                DebugManager.Instance.ErrorLog(SoundManager.Instance, "SEの再生レイヤーが足りていません。");
                return false;
            }

            if (!mClipDict.ContainsKey(name))
            {
                DebugManager.Instance.ErrorLog(SoundManager.Instance, "そのようなSEは存在しません");
                return false;
            }

            // 指定されたSEを流す
            SeSource.clip = mClipDict[name];
            SeSource.volume = _volume;
            SeSource.transform.position = pos;
            SeSource.Play();
            return true;
        }

    }
    [Serializable]
    class BGMController : AudioClass
    {
        string mNowBGM = "";

        public override bool Play(string name, Vector3 pos, float _volume = 1f)
        {
            if (!mClipDict.ContainsKey(name))
            {
                DebugManager.Instance.ErrorLog(SoundManager.Instance, "そのようなBGMは存在しません");
                return false;
            }

            // volumeを0から1の間になるようにする
            _volume = Mathf.Clamp(_volume, 0.0f, 1.0f);

            List<AudioSource> BGMSourceList = SearchUnUseSourceList();
            List<AudioSource> BGMNowSourceList = SearchUseSourceList();

            // 今流れているBGMと同じ場合音量だけ変更する
            if (name == mNowBGM)
            {
                foreach(var source in BGMSourceList)
                {
                    if(source.clip == mClipDict[name])
                    {
                        source.volume = _volume;
                    }
                }
                return true;
            }

            if (BGMSourceList.Count == 0)
            {
                DebugManager.Instance.ErrorLog(SoundManager.Instance, "BGMの再生レイヤーが足りていません。");
                return false;
            }

            BGMSourceList[0].clip = mClipDict[name];
            mNowBGM = name;

            // 今流れているBGMがある場合はフェードする
            if (BGMNowSourceList.Count > 0)
            {
                BGMSourceList[0].volume = _volume;
                BGMSourceList[0].Play();
                foreach (var source in BGMNowSourceList)
                    SoundManager.Instance.StartCoroutine(Fade(source, 0f, 0.4f));
                SoundManager.Instance.StartCoroutine(Fade(BGMSourceList[0], _volume, 0.4f));
            }
            // 今流れているBGMがない場合はいきなり流す
            else
            {
                BGMSourceList[0].volume = _volume;
                BGMSourceList[0].Play();
            }

            return true;
        }

    }
}
