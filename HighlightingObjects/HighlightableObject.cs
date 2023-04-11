using System;
using UnityEngine;

namespace UnityUtils
{
    /// <summary>
    /// Highlights a game object by changing its materials.
    /// Changes the materials back to their original state when the highlight is removed.
    /// Only works with otherwise non-changing materials.
    /// </summary>
    public class HighlightableObject : MonoBehaviour
    {
        private int _highlightCounter;
        private bool IsHighlighted => _highlightCounter > 0;
        private Renderer[] _renderers;
        private MaterialState[][] _materialStatesBeforeHighlighting;

        [SerializeField] private MaterialState highlightedMaterialState;
        [SerializeField] private bool materialsInChildren = true;

        /// <summary>
        /// Set whether the object should be highlighted or not.
        ///
        /// If HighlightWithCounter was used before, this method will always set the highlight counter back to either 1 or 0,
        /// depending on the value of the highlight parameter.
        /// </summary>
        /// <param name="highlight"> Whether to highlight the object or not. </param>
        public void Highlight(bool highlight)
        {
            Debug.Log("highlight = " + highlight);
        
            var wasHighlighted = IsHighlighted;
        
            _highlightCounter = highlight ? 1 : 0;
        
            if (wasHighlighted != IsHighlighted)
                UpdateHighlight();
        }
    
        /// <summary>
        /// Increase or decrease the highlight counter.
        /// 
        /// Calling this method with highlight == true will increase the highlight counter by 1,
        /// while calling it with highlight == false will decrease the highlight counter by 1 (to a minimum of 0).
        /// 
        /// Highlighting will only be disabled when the highlight counter reaches 0.
        ///
        /// Use this method, if multiple reasons can cause the object to be highlighted, and only once every source of
        /// highlighting is done, the object should stop being highlighted.
        /// </summary>
        /// <param name="highlight"> Whether to increase or decrease the counter. </param>
        /// <returns>True, if the object is (still) highlighted after this method call, False otherwise</returns>
        public bool HighlightWithCounter(bool highlight)
        {
            if (highlight)
                _highlightCounter++;
            else
                _highlightCounter--;

            if (_highlightCounter <= 0)
            {
                _highlightCounter = 0;
                return false;
            }
            return true;
        }

        private void UpdateHighlight()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                for (int j = 0; j < _renderers[i].materials.Length; j++)
                {
                    var newMaterialProperties = IsHighlighted ? highlightedMaterialState : _materialStatesBeforeHighlighting[i][j];
                    newMaterialProperties.ApplyToMaterial(_renderers[i].materials[j]);
                }
            
            
            }
        }

        private void Awake()
        {
            _renderers = materialsInChildren ? GetComponentsInChildren<Renderer>() : GetComponents<Renderer>();
            _materialStatesBeforeHighlighting = new MaterialState[_renderers.Length][];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _materialStatesBeforeHighlighting[i] = new MaterialState[_renderers[i].materials.Length];
                _materialStatesBeforeHighlighting[i] = MaterialState.CreateFromRenderer(_renderers[i], highlightedMaterialState);
            }
        
        }
    }

    /// <summary>
    /// All the properties that can potentially be changed in a material to highlight it.
    /// Extend this struct to add more properties.
    /// </summary>
    [Serializable]
    internal struct MaterialState
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Material material;
    
        [Header("Properties used for highlighting")]
        public bool changeEmission;
        public bool changeColor;

        public void ApplyToMaterial(Material rendererMaterial)
        {
            if (changeEmission)
            {
                if (material.IsKeywordEnabled("_EMISSION"))
                    rendererMaterial.EnableKeyword("_EMISSION");
                else
                    rendererMaterial.DisableKeyword("_EMISSION");

                rendererMaterial.SetColor(EmissionColor, material.GetColor(EmissionColor));
            }

            if (changeColor)
            {
                rendererMaterial.color = material.color;
            }

        }
    
        public static MaterialState[] CreateFromRenderer(Renderer renderer, MaterialState highlightedMaterialState)
        {
            var materialStates = new MaterialState[renderer.materials.Length];
            for (int i = 0; i < materialStates.Length; i++)
            {
                materialStates[i] = CreateFromMaterial(renderer.materials[i], highlightedMaterialState);
            }

            return materialStates;
        }
    
        public static MaterialState CreateFromMaterial(Material otherMaterial, MaterialState highlightedMaterialState)
        {
            return new MaterialState
            {
                material = new Material(otherMaterial),

                // only change the properties that are changed when highlighting
                changeEmission = highlightedMaterialState.changeEmission,
                changeColor = highlightedMaterialState.changeColor,
            };
        }
    }
}