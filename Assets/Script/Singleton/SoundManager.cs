using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    protected override bool dontDestroyOnLoad { get { return true; } }

    const float EnphasisFadeTime = 0.5f;

    #region �����o�ϐ�
    [SerializeField]
    SEController mSEController;
    [SerializeField]
    BGMController mBGMController;
    #endregion // �����o�ϐ�

    #region override
    protected override void Awake()
    {
        base.Awake();
        mSEController.Init();
        mBGMController.Init();
        DebugLogSystem.DebugLog(this, "generate SoundManager");
    }
    #endregion // override

    #region SE��public�֐�
    /// <summary>
    /// �}�X�^�[�{�����[������
    /// </summary>
    /// <param name="_rate">���ʊ���(0.0f - 1.0f)</param>
    public void setMasterVolume(float _rate)
    {
        AudioClass.setMasterVolume(_rate);
    }

    /// <summary>
    /// SE�Đ�(�����V���b�g)
    /// </summary>
    /// <param name="name">SE�̖��O</param>
    /// <param name="pos">�Đ��ʒu</param>
    /// <param name="_volume">���ʊ���(0.0f - 1.0f)</param>
    public void SEPlay(string name, Vector3 pos, float _volume = 1.0f)
    {
        mSEController.Play(name, pos, _volume);
    }

    /// <summary>
    /// SE�Đ�(���[�v)
    /// </summary>
    /// <param name="name">SE�̖��O</param>
    /// <param name="pos">�Đ��ʒu</param>
    /// <param name="_volume">���ʊ���(0.0f - 1.0f)</param>
    public void SEPlayLoop(string name, Vector3 pos, float _volume = 1.0f)
    {
        mSEController.Play(name, pos, _volume, true);
    }

    /// <summary>
    /// ��������SE�̍Đ�(BGM�̉��ʂ������Ă���Đ�����)
    /// </summary>
    /// <param name="name">SE�̖��O</param>
    /// <param name="pos">�Đ��ʒu</param>
    /// <param name="_volume">���ʊ���(0.0f - 1.0f)</param>
    /// <param name="_isloop">���[�v���邩</param>
    /// <param name="_bgmVolume">BGM�̉��ʂ��ǂꂭ�炢�����邩</param>
    public void SEPlayEnphasis(string name, Vector3 pos, float _volume, bool _isloop, float _bgmVolume)
    {
        StartCoroutine(SEPlayEmphasis(name, pos, _volume, _isloop, _bgmVolume));
    }

    /// <summary>
    /// SE�̃{�����[���̃Z�b�^�[
    /// </summary>
    /// <param name="_volume">���ʊ���(0.0f - 1.0f)</param>
    public void SetSEVolume(float _volume)
    {
        mSEController.setVolume(_volume);
    }

    /// <summary>
    /// SE�̃t�F�[�h�A�E�g
    /// </summary>
    /// <param name="_name">SE�̖��O</param>
    /// <param name="_fadeTime">�t�F�[�h���鎞��</param>
    public void FadeOutSE(string _name, float _fadeTime)
    {
        mSEController.FadeOut(_name, _fadeTime);
    }

    #endregion // SE��public�֐�

    #region BGM��public�֐�
    /// <summary>
    /// BGM�̍Đ�
    /// </summary>
    /// <param name="name">BGM�̖��O</param>
    /// <param name="pos">�Đ��ꏊ</param>
    /// <param name="_volume">���ʊ���(0.0f - 1.0f)</param>
    public void BGMPlay(string name, Vector3 pos, float _volume)
    {
        mBGMController.Play(name, pos, _volume);
    }

    /// <summary>
    /// BGM�̉��ʂ̃Z�b�^�[
    /// </summary>
    /// <param name="_volume">���ʊ���(0.0f - 1.0f)</param>
    public void SetBGMVolume(float _volume)
    {
        mBGMController.setVolume(_volume);
    }

    /// <summary>
    /// ���݂�BGM�̃t�F�[�h
    /// ���݂̉��ʂƔ�ׂď�������΃t�F�[�h�A�E�g
    /// �Đ�����Ă��Ȃ��܂��͌��݂̉��ʂƔ�ׂđ傫����΃t�F�[�h�C��
    /// </summary>
    /// <param name="_volume">���ʊ���(0.0f - 1.0f)</param>
    /// <param name="_fadeTime">�t�F�[�h����</param>
    public void FadeBGM(float _volume, float _fadeTime)
    {
        mBGMController.Fade(_volume, _fadeTime);
    }
    #endregion // BGM��public�֐�

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
            // ���X�g��Dictionary�`���ɕύX
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
        ///  �g�p����Ă��Ȃ�AudioSource��T��
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
        ///  �g�p����Ă���AudioSource��T��
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
        /// �w�肵��AudioSource�̉���volume�܂Ńt�F�[�h����
        /// ���ʂ�0�܂ōs����AudioSource���Ƃ߂�
        /// AudioSource���Ȃ��Ă��Ȃ��ꍇ�͉���炵�n�߂�
        /// </summary>
        protected IEnumerator Fade(AudioSource _source, float _volume, float _fadeTime)
        {
            if (_source.volume == _volume)
                yield break;

            // volume��0����1�̊ԂɂȂ�悤�ɂ���
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
            // volume��0����1�̊ԂɂȂ�悤�ɂ���
            _volume = Mathf.Clamp(_volume, 0.0f, 1.0f);
            _volume *= getVolumeRate;
            // �g���Ă��Ȃ�AudioSource��T��
            AudioSource SeSource = SearchUnUseSourceList()[0];

            if (SeSource == null)
            {
                DebugLogSystem.ErrorLog(SoundManager.Instance, "SE�̍Đ����C���[������Ă��܂���B");
                return null;
            }

            if (!mClipDict.ContainsKey(name))
            {
                DebugLogSystem.ErrorLog(SoundManager.Instance, "���̂悤��SE�͑��݂��܂���");
                return null;
            }

            // �w�肳�ꂽSE�𗬂�
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
                DebugLogSystem.ErrorLog(SoundManager.Instance, "���̂悤��BGM�͑��݂��܂���");
                return null;
            }

            // volume��0����1�̊ԂɂȂ�悤�ɂ���
            _volume = Mathf.Clamp(_volume, 0.0f, 1.0f);
            _volume *= getVolumeRate;

            List<AudioSource> BGMSourceList = SearchUnUseSourceList();
            List<AudioSource> BGMNowSourceList = SearchUseSourceList();

            // ������Ă���BGM�Ɠ����ꍇ���ʂ����ύX����
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
                DebugLogSystem.ErrorLog(SoundManager.Instance, "BGM�̍Đ����C���[������Ă��܂���B");
                return null;
            }

            BGMSourceList[0].clip = mClipDict[name];
            mNowBGM = name;

            // ������Ă���BGM������ꍇ�̓t�F�[�h����
            if (BGMNowSourceList.Count > 0)
            {
                BGMSourceList[0].volume = _volume;
                BGMSourceList[0].Play();
                foreach (var source in BGMNowSourceList)
                    SoundManager.Instance.StartCoroutine(Fade(source, 0f, 0.4f));
                SoundManager.Instance.StartCoroutine(Fade(BGMSourceList[0], _volume, 0.4f));
            }
            // ������Ă���BGM���Ȃ��ꍇ�͂����Ȃ藬��
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
