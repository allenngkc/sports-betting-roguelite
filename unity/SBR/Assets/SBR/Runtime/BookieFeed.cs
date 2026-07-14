using System.Collections.Generic;
using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// M5's thin room adapter: polls the director's immutable presentation snapshot and lets the pure
    /// model decide what the bookie says. Only the phone's own DeskFocus marks messages read; looking
    /// at the laptop is not consent to clear the phone badge.
    /// </summary>
    public sealed class BookieFeed : MonoBehaviour
    {
        [Header("Wiring (set by GrayboxRoomBuilder)")]
        public RunDirector director;
        public DeskFocus phoneFocus;

        private readonly BookieFeedModel _model = new BookieFeedModel();

        public IReadOnlyList<BookieMessage> Messages => _model.Messages;
        public int UnreadCount => _model.UnreadCount;
        public long Revision => _model.Revision;
        public long ArrivalSequence => _model.ArrivalSequence;

        private void Update()
        {
            if (director == null || director.Run == null)
                return;

            _model.Observe(director.RunGeneration, director.Run, director.Run.Phase,
                director.Run.Round, director.LastSettle);

            if (phoneFocus != null && DeskFocus.Active == phoneFocus)
                _model.MarkRead();
        }
    }
}
