#region Copyright
// This is an unpublished work protected under the copyright laws of the United
// States and other countries.  All rights reserved.  Should publication occur
// the following will apply:  © 2011 GameTech International, Inc.
#endregion

using System;
using GameTech.Elite.Base;
using GameTech.Elite.Client.Modules.SessionSummary.Business;

namespace GameTech.Elite.Client.Modules.SessionSummary.ViewModel
{
    /// <summary>
    /// View model class to provide the ability to make a session costs selectable
    /// </summary>
    internal class SessionCostSelectionViewModel : ViewModelBase
    {
        #region Member Variables
        private SessionCost m_cost;
        private bool m_isSelected;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a SessionCostSelectionViewModel
        /// </summary>
        /// <param name="cost">The session cost.</param>
        public SessionCostSelectionViewModel(SessionCost cost)
        {
            if (cost == null)
                throw new ArgumentNullException("cost");

            m_cost = cost;
        }

        /// <summary>
        /// Initializes a SessionCostSelectionViewModel
        /// </summary>
        /// <param name="cost">the cost.</param>
        /// <param name="selected">whether the item is selected or not.</param>
        public SessionCostSelectionViewModel(SessionCost cost, bool selected)
        {
            if (m_cost == null)
                throw new ArgumentNullException("cost");

            m_cost = cost;
            IsSelected = selected;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Converts this objects data to a string.
        /// </summary>
        /// <returns>The object information.</returns>
        public override string ToString()
        {
            return m_cost.Name;
        }

        #endregion

        #region Member Properties
        /// <summary>
        /// Gets the cost associated with the view model.
        /// </summary>
        public SessionCost Cost
        {
            get
            {
                return m_cost;
            }
            private set
            {
                if (m_cost != value)
                {
                    m_cost = value;
                    RaisePropertyChanged("Cost");
                }
            }
        }

        /// <summary>
        /// Gets the name of the cost
        /// </summary>
        public string Name
        {
            get
            {
                return m_cost.Name;
            }
        }

        /// <summary>
        /// Gets or sets whether the view model is selected
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }
        #endregion
    }
}
