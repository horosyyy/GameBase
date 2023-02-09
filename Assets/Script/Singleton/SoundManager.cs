using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    const float EnphasisFadeTime = 0.5f;

    #region メンバ変数
    [SerializeField]
    SEController mSEController;
    [SerializeField]
    BGMController mBGMController;
    #endregion // メンバ変数

    #region override
    protected override void Awake()
    {
        base.Awake();
        mSEController.Init();
        mBGMController.Init();
        DebugLogSystem.DebugLog(this, "generate SoundManager");
    }
    #endregion // override

    #region SEのpublic関数
    /// <summary>
    /// マスターボリューム調節
    /// </summary>
    /// <param name="_rate">音量割合(0.0f - 1.0f)</param>
    public void setMasterVolume(float _rate)
    {
        AudioClass.setMasterVolume(_rate);
    }

    /// <summary>
    /// SE再生(ワンショット)
    /// </summary>
    /// <param name="name">SEの名前</param>
    /// <param name="pos">再生位置</param>
    /// <param name="_volume">音量割合(0.0f - 1.0f)</param>
    public void SEPlay(string name, Vector3 pos, float _volume = 1.0f)
    {
        mSEController.Play(name, pos, _volume);
    }

    /// <summary>
    /// SE再生(ループ)
    /// </summary>
    /// <param name="name">SEの名前</param>
    /// <param name="pos">再生位置</param>
    /// <param name="_volume">音量割合(0.0f - 1.0f)</param>
    public void SEPlayLoop(string name, Vector3 pos, float _volume = 1.0f)
    {
        mSEController.Play(name, pos, _volume, true);
    }

    /// <summary>
    /// 強調するSEの再生(BGMの音量を下げてから再生する)
    /// </summary>
    /// <param name="name">SEの名前</param>
    /// <param name="pos">再生位置</param>
    /// <param name="_volume">音量割合(0.0f - 1.0f)</param>
    /// <param name="_isloop">ループするか</param>
    /// <param name="_bgmVolume">BGMの音量をどれくらい下げるか</param>
    public void SEPlayEnphasis(string name, Vector3 pos, float _volume, bool _isloop, float _bgmVolume)
    {
        StartCoroutine(SEPlayEmphasis(name, pos, _volume, _isloop, _bgmVolume));
    }

    /// <summary>
    /// SEのボリュームのセッター
    /// </summary>
    /// <param name="_volume">音量割合(0.0f - 1.0f)</param>
    public void SetSEVolume(float _volume)
    {
        mSEController.setVolume(_volume);
    }

    /// <summary>
    /// SEのフェードアウト
    /// </summary>
    /// <param name="_name">SEの名前</param>
    /// <param name="_fadeTime">フェードする時間</param>
    public void FadeOutSE(string _name, float _fadeTime)
    {
        mSEController.FadeOut(_name, _fadeTime);
    }

    #endregion // SEのpublic関数

    #region BGMのpublic関数
    /// <summary>
    /// BGMの再生
    /// </summary>
    /// <param name="name">BGMの名前</param>
    /// <param name="pos">再生場所</param>
    /// <param name="_volume">音量割合(0.0f - 1.0f)</param>
    public void BGMPlay(string name, Vector3 pos, float _volume)
    {
        mBGMController.Play(name, pos, _volume);
    }

    /// <summary>
    /// BGMの音量のセッター
    /// </summary>
    /// <param name="_volume">音量割合(0.0f - 1.0f)</param>
    public void SetBGMVolume(float _volume)
    {
        mBGMController.setVolume(_volume);
    }

    /// <summary>
    /// 現在のBGMのフェード
    /// 現在の音量と比べて小さければフェードアウト
    /// 再生されていないまたは現在の音量と比べて大きければフェードイン
    /// </summary>
    /// <param name="_volume">音量割合(0.0f - 1.0f)</param>
    /// <param name="_fadeTime">フェード時間</param>
    public void FadeBGM(float _volume, float _fadeTime)
    {
        mBGMController.Fade(_volume, _fadeTime);
    }
    #endregion // BGMのpublic関数

    #region private
    private IEnumerator SEPlayEmphasis(string name, Vector3 pos, float _volume, bool _isloop, float _bgmVolume)
    {
        yield return mBGMController.SEEnphasisFade(_bgmVolume, EnphasisFadeTime, SEPlayCoroutine());
        yield break;
        IEnumerator SEPlayCoroutine()
        {
            var se = mSEController.Play(name, pos, _volume, _isloop);
            while (se.isPlaying) yield return null;
            yield break;
        }
    }
    #endregion

    #region AudioClass

    [Serializable]
    abstract class AudioClass
    {
        [Serializable]
        public struct ClipInfo
        {
            public string name;
            public AudioClip clip;
        }

        private float mVolumeRate = 1.0f;
        private static float mMasterVolumeRate = 1.0f;
        public float getVolumeRate { get { return mVolumeRate * mMasterVolumeRate; } }
        

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

        public static void setMasterVolume(float _rate)
        {
            mMasterVolumeRate = _rate;
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
        public void FadeOut(string _name, float _fadeTime)
        {
            foreach (var source in mSourceList)
            {
                if (source.isPlaying && source.clip == mClipDict[_name])
                {
                    SoundManager.Instance.StartCoroutine(Fade(source, 0.0f, _fadeTime));
                }
            }
        }

        public abstract AudioSource Play(string name, Vector3 pos, float _volume, bool _isloop);

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

            // volumeを0から1の間になるようにする
            _volume = Mathf.Clamp(_volume, 0.0f, 1.0f);
            _volume *= getVolumeRate;

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
        public override AudioSource Play(string name, Vector3 pos, float _volume = 1f, bool _isloop = false)
        {
            // volumeを0から1の間になるようにする
            _volume = Mathf.Clamp(_volume, 0.0f, 1.0f);
            _volume *= getVolumeRate;
            // 使っていないAudioSourceを探す
            AudioSource SeSource = SearchUnUseSourceList()[0];

            if (SeSource == null)
            {
                DebugLogSystem.ErrorLog(SoundManager.Instance, "SEの再生レイヤーが足りていません。");
                return null;
            }

            if (!mClipDict.ContainsKey(name))
            {
                DebugLogSystem.ErrorLog(SoundManager.Instance, "そのようなSEは存在しません");
                return null;
            }

            // 指定されたSEを流す
            SeSource.clip = mClipDict[name];
            SeSource.volume = _volume;
            SeSource.transform.position = pos;
            SeSource.loop = _isloop;
            SeSource.Play();
            return SeSource;
        }
    }

    [Serializable]
    class BGMController : AudioClass
    {
        string mNowBGM = "";

        public override AudioSource Play(string name, Vector3 pos, float _volume = 1f, bool _isloop = true)
        {
            if (!mClipDict.ContainsKey(name))
            {
                DebugLogSystem.ErrorLog(SoundManager.Instance, "そのようなBGMは存在しません");
                return null;
            }

            // volumeを0から1の間になるようにする
            _volume = Mathf.Clamp(_volume, 0.0f, 1.0f);
            _volume *= getVolumeRate;

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
                        return source;
                    }
                }
            }

            if (BGMSourceList.Count == 0)
            {
                DebugLogSystem.ErrorLog(SoundManager.Instance, "BGMの再生レイヤーが足りていません。");
                return null;
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
            BGMSourceList[0].loop = _isloop;

            return BGMSourceList[0];
        }

        public void Fade(float _volume, float _fadeTime)
        {
            AudioSource bgm = SearchNowPlayingSource();
            if (bgm != null)
            {
                bgm = SearchUnUseSourceList()[0];
            }
            SoundManager.Instance.StartCoroutine(Fade(bgm, _volume, _fadeTime));
        }

        public IEnumerator SEEnphasisFade(float _volume, float _fadeTime, IEnumerator SEPlayCoroutine)
        {
            AudioSource bgm = SearchNowPlayingSource();
            float originVolume = bgm.volume;
            if(bgm == null)
            {
                yield return SoundManager.Instance.StartCoroutine(SEPlayCoroutine);
                yield break;
            }
            yield return Fade(bgm, _volume, _fadeTime);
            yield return SoundManager.Instance.StartCoroutine(SEPlayCoroutine);
            yield return Fade(bgm, originVolume, _fadeTime);
            yield break;

        }

        private AudioSource SearchNowPlayingSource()
        {
            AudioSource bgm = null;
            var useSourceList = SearchUseSourceList();
            foreach (var source in useSourceList)
            {
                if (source.clip == mClipDict[mNowBGM])
                {
                    bgm = source;
                }
            }
            return bgm;
        }

    }

    #endregion
}
