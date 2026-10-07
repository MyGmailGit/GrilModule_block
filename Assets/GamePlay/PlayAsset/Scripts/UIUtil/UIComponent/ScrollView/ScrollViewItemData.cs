using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIUtil.UI
{
    public class ScrollViewItemData : IScrollViewItemDataHandler
    {
        protected int m_index;
        public int index
        {
            set { m_index = value; }
            get { return m_index; }
        }
        protected string m_prefabName;
        protected object m_data;

        protected bool m_isSizeValid;
        protected Vector2 m_size;
        public Vector2 size
        {
            set
            {
                m_size = value;
                m_isSizeValid = true;
            }
            get { return m_size; }
        }

        protected bool m_isPresetPosValid;
        protected Vector2 m_presetPos;
        public Vector2 presetPos
        {
            set
            {
                m_presetPos = value;
                m_isPresetPosValid = true;
            }
            get { return m_presetPos; }
        }

        protected bool m_isSpace = false;

        public ScrollViewItemData(string _prefabName, object _data)
        {
            m_data = _data;
            m_prefabName = _prefabName;
            m_isSizeValid = false;
            m_isPresetPosValid = false;
            m_isSpace = false;
        }

        public ScrollViewItemData(string _prefabName, object _data, Vector2 _size)
        {
            m_data = _data;
            m_prefabName = _prefabName;
            m_size = _size;
            m_isSizeValid = true;
            m_isPresetPosValid = false;
            m_isSpace = false;
        }

        public ScrollViewItemData(string _prefabName, object _data, Vector2 _size, bool _isSpace)
        {
            m_data = _data;
            m_prefabName = _prefabName;
            m_size = _size;
            m_isSizeValid = true;
            m_isPresetPosValid = false;
            m_isSpace = _isSpace;
        }

        public virtual string GetPrefabName()
        {
            return m_prefabName;
        }
        public virtual object GetData()
        {
            return m_data;
        }
        public virtual Vector2 GetSize()
        {
            return Vector2.zero;
        }
        public bool IsSizeValid()
        {
            return m_isSizeValid;
        }
        public bool IsPresetPosValid()
        {
            return m_isPresetPosValid;
        }
        public virtual bool IsSpaceItem()
        {
            return m_isSpace;
        }
    }

    public abstract class BaseList
    {
        protected ListType m_listType = ListType.Base;

        public ListType GetListType()
        {
            return m_listType;
        }
    }

    public class ImageList : BaseList
    {
        public List<Color> list = new List<Color>();
        public ImageList(List<Color> _data) : base()
        {
            list = _data;
            m_listType = ListType.Image;
        }
    }

    public class TextList : BaseList
    {
        public List<string> list = new List<string>();
        public TextList(List<string> _data) : base()
        {
            list = _data;
            m_listType = ListType.Text;
        }
    }

    public class CircleImageList : BaseList
    {
        public List<Color> list = new List<Color>();
        public CircleImageList(List<Color> _data) : base()
        {
            list = _data;
            m_listType = ListType.CircleImage;
        }
    }

    public enum ListType
    {
        Base,
        Image,
        Text,
        CircleImage,
    }
}