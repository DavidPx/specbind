// <copyright file="WaitForListItemsAction.cs">
//    Copyright © 2014 Dan Piessens.  All rights reserved.
// </copyright>

namespace SpecBind.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using SpecBind.ActionPipeline;
    using SpecBind.Pages;

    /// <summary>
    /// An action that waits for a list element to contain items.
    /// </summary>
    public class WaitForListItemsAction : ContextActionBase<WaitForListItemsAction.WaitForListItemsContext>
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitForListItemsAction" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public WaitForListItemsAction(ILogger logger)
            : base(typeof(WaitForListItemsAction).Name)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Executes this instance action.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <returns>The result of the action.</returns>
        protected override ActionResult Execute(WaitForListItemsContext actionContext)
        {
            // Get the element
            var propertyName = actionContext.PropertyName;
            var element = this.ElementLocator.GetProperty(propertyName);

            // Make sure the element is a list
            if (!element.IsList)
            {
                var exception =
                    new ElementExecuteException(
                        "Property '{0}' is not a list and cannot be used in this wait.",
                        propertyName);

                return ActionResult.Failure(exception);
            }
            
            // Setup timeout items
            var timeout = actionContext.Timeout.GetValueOrDefault(TimeSpan.FromSeconds(20));
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(timeout);
            var token = cancellationTokenSource.Token;

            try
            {
				var task = Task.Run(() => this.CheckList(element, token, actionContext.NumberOfItems), token);
                task.Wait(token);

                return ActionResult.Successful();
            }
            catch (OperationCanceledException)
            {
				var exception = new PageNavigationException("List '{0}' did not contain {2} elements after {1}", propertyName, timeout, actionContext.NumberOfItems);
                return ActionResult.Failure(exception);
            }
        }

        /// <summary>
		/// Checks the list for correctness.
        /// </summary>
        /// <param name="listElement">the list element.</param>
        /// <param name="token">The cancellation token.</param>
		/// <param name="numberOfItems"></param>
		private void CheckList(IPropertyData listElement, CancellationToken token, int numberOfItems)
        {
            while (true)
            {
				var item = listElement.GetItemAtIndex(numberOfItems - 1);
                if (item != null)
                {
                    return;
                }

				this.logger.Debug("List did not contain at least {0} elements, waiting...", numberOfItems);
                token.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(500));
                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// An action context for the action.
        /// </summary>
        public class WaitForListItemsContext : ActionContext
        {
            /// <summary>
			/// The number of items that should be in the list; default behavior is to wait for one item to be in the list
			/// </summary>
			public readonly int NumberOfItems;

			/// <summary>
			/// Initializes a new instance of the <see cref="WaitForListItemsContext" /> class.
            /// </summary>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="timeout">The timeout.</param>
            public WaitForListItemsContext(string propertyName, TimeSpan? timeout)
                : base(propertyName)
            {
				this.NumberOfItems = 1;
                this.Timeout = timeout;
            }

            /// <summary>
			/// Initializes a <see cref="WaitForListItemsContext"/> that will wait for a specifified number of items to be in the list
			/// </summary>
			/// <param name="propertyName"></param>
			/// <param name="timeout"></param>
			/// <param name="numberOfItems"></param>
			public WaitForListItemsContext(string propertyName, TimeSpan? timeout, int numberOfItems)
				: this(propertyName, timeout)
			{
				if (numberOfItems < 1)
					throw new ArgumentOutOfRangeException("numberOfItems", "The number of items should be 1 or greater");

				this.NumberOfItems = numberOfItems;
			}

			/// <summary>
            /// Gets the timeout.
            /// </summary>
            /// <value>The timeout.</value>
            public TimeSpan? Timeout { get; private set; }
        }
    }
}