// Profile screen demo
import { Handle } from './handle.js';
import { UIManager } from './ui.js';

// Initialize and export
const UI = new UIManager();

function listenIfExists(selector, event, handler) {
    const element = document.querySelector(selector);
    if (element) {
        element.addEventListener(event, handler);
    }
}

function listenWindow(event, handler) {
    if (typeof handler === 'function') {
        window.addEventListener(event, handler);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    // Existing login and signup listeners remain unchanged...

    // Profile Edit Save
    listenIfExists('#saveEdit', 'click', async () => {
        const editName = document.getElementById('editName')?.value || '';
        const editPassword = document.getElementById('editPassword')?.value || '';
        const editEmail = document.getElementById('editEmail')?.value || '';
        const editAvatar = document.getElementById('editAvatar')?.files[0];
        const usernameDisplay = document.getElementById('usernameDisplay');

        UI.showLoading('Updating profile...');
        const success = await Handle.UpdateProfile(editName, editPassword, editEmail, editAvatar);
        UI.hideLoading();

        if (success) {
            UI.showSuccess('Profile updated successfully!');
            if (editName) usernameDisplay.textContent = editName;
            // Refresh profile data
            await Handle.LoadProfile();
        } else {
            UI.showError('Failed to update profile.');
        }
    });

    // Cancel Edit
    listenIfExists('#cancelEdit', 'click', () => {
        document.getElementById('editMode').classList.add('hidden');
        UI.hideLoading();
    });

    // Load profile data on page load
    listenWindow('load', async () => {
        const path = window.location.pathname;
        let fileName = path.substring(path.lastIndexOf('/') + 1);
        const token = localStorage.getItem('token');
        if (token && (fileName === Pages.INDEX || fileName === Pages.AUTH || fileName === Pages.REGISTER)) {
            UI.goTo(Pages.PROFILE);
        }
        if (fileName === Pages.PROFILE) {
            UI.showLoading('Loading profile...');
            await Handle.LoadProfile();
            UI.hideLoading();
        }
        setTimeout(() => UI.hideLoading(), 500);
    });
});

// Game screen demo
document.addEventListener('DOMContentLoaded', () => {
  const userId = localStorage.getItem('userId'); // Assume userId is stored

  function getCurrentFilters() {
    return {
      genre: document.getElementById('genreFilter')?.value || 'All Genres',
      platform: document.getElementById('platformFilter')?.value || 'All Platforms'
    };
  }

  Handle.listenIfExists = function(selector, event, callback) {
    const element = document.querySelector(selector);
    if (element) element.addEventListener(event, callback);
  };

  Handle.listenIfExists('#genreFilter', 'change', () => Handle.handleLoggedGamesFetch(userId, getCurrentFilters(), document.getElementById('sortOption').value, document.getElementById('reviewedFilter').checked));
  Handle.listenIfExists('#platformFilter', 'change', () => Handle.handleLoggedGamesFetch(userId, getCurrentFilters(), document.getElementById('sortOption').value, document.getElementById('reviewedFilter').checked));
  Handle.listenIfExists('#sortOption', 'change', () => Handle.handleLoggedGamesFetch(userId, getCurrentFilters(), document.getElementById('sortOption').value, document.getElementById('reviewedFilter').checked));
  Handle.listenIfExists('#reviewedFilter', 'change', () => Handle.handleLoggedGamesFetch(userId, getCurrentFilters(), document.getElementById('sortOption').value, document.getElementById('reviewedFilter').checked));
  Handle.listenIfExists('#loadMoreGames', 'click', () => Handle.handleLoggedGamesFetch(userId, getCurrentFilters(), document.getElementById('sortOption').value, document.getElementById('reviewedFilter').checked, true));
  Handle.listenIfExists('.edit-profile', 'click', () => Handle.handleProfileGamesUpdate(userId, UI.collectProfileData()));

  // Initial load
  if (userId) Handle.handleProfileGamesFetch(userId);
  if (userId) Handle.handleLoggedGamesFetch(userId, getCurrentFilters(), 'Date Logged', false);
});

// Reviews screen demo
function listenIfExists(selector, event, callback) {
    const element = document.querySelector(selector);
    if (element) element.addEventListener(event, callback);
}

document.addEventListener('DOMContentLoaded', () => {
    const userId = 'mockUserId'; // Replace with actual user ID retrieval
    Handle.handleReviewsFetch(userId);

    // Filter events
    listenIfExists('#genreFilter', 'change', () => {
        const filters = {
            genre: genreFilter.value,
            platform: platformFilter.value,
            search: searchInput.value.toLowerCase()
        };
        Handle.handleFilterReviews(filters);
    });
    listenIfExists('#platformFilter', 'change', () => {
        const filters = {
            genre: genreFilter.value,
            platform: platformFilter.value,
            search: searchInput.value.toLowerCase()
        };
        Handle.handleFilterReviews(filters);
    });
    listenIfExists('#sortFilter', 'change', () => Handle.handleSortReviews(sortFilter.value));
    listenIfExists('#clearFilters', 'click', () => {
        genreFilter.value = '';
        platformFilter.value = '';
        sortFilter.value = 'date_desc';
        searchInput.value = '';
        Handle.handleFilterReviews({});
    });

    // Search functionality
    listenIfExists('#searchInput', 'input', () => {
        const filters = {
            genre: genreFilter.value,
            platform: platformFilter.value,
            search: searchInput.value.toLowerCase()
        };
        Handle.handleFilterReviews(filters);
    });
    listenIfExists('#searchInput', 'focus', showSearchResults);
    listenIfExists('#searchInput', 'blur', hideSearchResults);

    // Load more
    listenIfExists('#loadMoreBtn', 'click', () => Handle.handleFilterReviews({
        genre: genreFilter.value,
        platform: platformFilter.value,
        search: searchInput.value.toLowerCase()
    }));

    // Modal events
    listenIfExists('#addReviewBtn', 'click', openAddReviewModal);
    listenIfExists('#closeModal', 'click', UI.closeModal);
    listenIfExists('#cancelReview', 'click', UI.closeModal);
    listenIfExists('#reviewForm', 'submit', (e) => {
        e.preventDefault();
        const reviewData = {
            title: document.getElementById('gameTitle').value,
            rating: parseInt(document.getElementById('ratingValue').value),
            content: document.getElementById('reviewText').value,
            tags: document.getElementById('reviewTags').value.split(',').map(tag => tag.trim()).filter(tag => tag)
        };
        if (reviewForm.onsubmit === handleAddReview) {
            Handle.handleAddReview('mockUserId', reviewData);
        } else {
            const reviewId = parseInt(document.querySelector('.edit-review-btn[data-review-id]:focus')?.dataset.reviewId || 0);
            Handle.handleUpdateReview('mockUserId', reviewId, reviewData);
        }
    });

    // Dynamic action buttons
    listenIfExists('.edit-review-btn', 'click', (e) => {
        const reviewId = parseInt(e.target.closest('[data-review-id]')?.dataset.reviewId);
        UI.openModal(reviews.find(r => r.id === reviewId));
    });
    listenIfExists('.delete-review-btn', 'click', (e) => {
        const reviewId = parseInt(e.target.closest('[data-review-id]')?.dataset.reviewId);
        Handle.handleDeleteReview('mockUserId', reviewId);
    });
    listenIfExists('.share-review-btn', 'click', (e) => {
        const reviewId = parseInt(e.target.closest('[data-review-id]')?.dataset.reviewId);
        shareReview(reviewId); // Using the existing function
    });
    listenIfExists('.like-btn', 'click', (e) => {
        const reviewId = parseInt(e.target.closest('[data-review-id]')?.dataset.reviewId);
        Handle.handleLikeReview('mockUserId', reviewId);
    });
});