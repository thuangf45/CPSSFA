// Profile screen demo
import { APIAuth, APIUser } from './api.js';
import { Pages, UIManager } from './ui.js';

// Initialize and export
const UI = new UIManager();

export class Handle {
    static async Login(username, password) {
        // Existing Login method remains unchanged...
    }

    static async Register(username, password, email) {
        // Existing Register method remains unchanged...
    }

    static async UpdateProfile(username, password, email, avatar) {
        const api = new APIUser();
        const res = await api.UpdateProfile(username, password, email, avatar);
        if (res.ok) {
            UI.updateAvatarPreview(avatar); // Update preview if avatar changed
            return true;
        }
        return false;
    }

    static async LoadProfile() {
        const api = new APIUser();
        const res = await api.GetProfile();
        if (res.ok && res.data) {
            UI.updateProfileDisplay(res.data);
            return true;
        }
        UI.showError('Failed to load profile.');
        return false;
    }
}

// Game screen demo
class Handle {
  static async handleProfileGamesFetch(userId) {
    UI.showLoading();
    try {
      const [profileData, stats] = await Promise.all([
        GameAPI.GetProfileGames(userId),
        GameAPI.GetStats(userId)
      ]);
      UI.updateProfileDisplay(profileData, stats);
    } catch (error) {
      UI.showNotification('Failed to load profile data');
    } finally {
      UI.hideLoading();
    }
  }

  static async handleLoggedGamesFetch(userId, filter = {}, sort = 'Date Logged', reviewedOnly = false, loadMore = false) {
    UI.showLoading();
    try {
      const games = await GameAPI.GetLoggedGames(userId, filter, sort, reviewedOnly);
      UI.addLoggedGamesToPage(games, loadMore);
    } catch (error) {
      UI.showNotification('Failed to load games');
    } finally {
      UI.hideLoading();
    }
  }

  static async handleProfileGamesUpdate(userId, data) {
    UI.showLoading();
    try {
      await GameAPI.PutProfileGames(userId, data);
      await Handle.handleProfileGamesFetch(userId); // Refresh display
      UI.showNotification('Profile updated successfully');
    } catch (error) {
      UI.showNotification('Failed to update profile');
    } finally {
      UI.hideLoading();
    }
  }
}

//Reviews screen demo
class Handle {
    static async handleReviewsFetch(userId) {
        UI.showLoading();
        try {
            const data = await GameAPI.GetReviews(userId);
            reviews = data; // Assume reviews is globally available as in the HTML
            filteredReviews = [...reviews];
            UI.renderReviews(displayedReviews);
            UI.updateReviewStats(reviews.length);
        } catch (error) {
            UI.showNotification('Failed to load reviews', 'error');
        } finally {
            UI.hideLoading();
        }
    }

    static async handleFilterReviews(filters) {
        UI.showLoading();
        try {
            const data = await GameAPI.GetFilteredReviews('mockUserId', filters, sortFilter.value);
            filteredReviews = data;
            currentPage = 1;
            UI.renderReviews(displayedReviews);
            UI.updateFilterStats();
        } catch (error) {
            UI.showNotification('Failed to filter reviews', 'error');
        } finally {
            UI.hideLoading();
        }
    }

    static async handleSortReviews(sort) {
        UI.showLoading();
        try {
            const filters = {
                genre: genreFilter.value,
                platform: platformFilter.value,
                search: searchInput.value.toLowerCase()
            };
            const data = await GameAPI.GetFilteredReviews('mockUserId', filters, sort);
            filteredReviews = data;
            currentPage = 1;
            UI.renderReviews(displayedReviews);
            UI.updateFilterStats();
        } catch (error) {
            UI.showNotification('Failed to sort reviews', 'error');
        } finally {
            UI.hideLoading();
        }
    }

    static async handleAddReview(userId, reviewData) {
        UI.showLoading();
        try {
            const data = await GameAPI.PostReview(userId, reviewData);
            reviews.unshift(data);
            filteredReviews = [...reviews];
            UI.renderReviews(displayedReviews);
            UI.updateReviewStats(reviews.length);
            UI.showNotification('Review added successfully!', 'success');
        } catch (error) {
            UI.showNotification('Failed to add review', 'error');
        } finally {
            UI.hideLoading();
            UI.closeModal();
        }
    }

    static async handleUpdateReview(userId, reviewId, reviewData) {
        UI.showLoading();
        try {
            const data = await GameAPI.PutReview(userId, reviewId, reviewData);
            const index = reviews.findIndex(r => r.id === reviewId);
            if (index !== -1) reviews[index] = data;
            filteredReviews = [...reviews];
            UI.renderReviews(displayedReviews);
            UI.updateReviewStats(reviews.length);
            UI.showNotification('Review updated successfully!', 'success');
        } catch (error) {
            UI.showNotification('Failed to update review', 'error');
        } finally {
            UI.hideLoading();
            UI.closeModal();
        }
    }

    static async handleDeleteReview(userId, reviewId) {
        UI.showLoading();
        try {
            await GameAPI.DeleteReview(userId, reviewId);
            reviews = reviews.filter(r => r.id !== reviewId);
            filteredReviews = [...reviews];
            UI.renderReviews(displayedReviews);
            UI.updateReviewStats(reviews.length);
            UI.showNotification('Review deleted successfully!', 'success');
        } catch (error) {
            UI.showNotification('Failed to delete review', 'error');
        } finally {
            UI.hideLoading();
        }
    }

    static async handleLikeReview(userId, reviewId) {
        UI.showLoading();
        try {
            const data = await GameAPI.PostReviewLike(userId, reviewId);
            const review = reviews.find(r => r.id === reviewId);
            if (review) review.likes += 1;
            UI.updateLikeCount(reviewId, review.likes);
            UI.showNotification('Review liked!', 'success');
        } catch (error) {
            UI.showNotification('Failed to like review', 'error');
        } finally {
            UI.hideLoading();
        }
    }
}