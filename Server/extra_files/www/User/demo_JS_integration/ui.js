// Profile screen demo
export class UIManager {
    constructor() {
        this.init();
    }

    init() {
        this.createNotificationContainer();
    }

    goTo(page) {
        window.location.href = page;
    }

    // Existing notification and loading methods remain unchanged...

    // Update to handle avatar preview
    updateAvatarPreview(file) {
        const avatarPreview = document.getElementById('avatarPreview');
        const headerAvatar = document.querySelector('header img');
        if (file && avatarPreview) {
            const reader = new FileReader();
            reader.onload = function(e) {
                avatarPreview.src = e.target.result;
                if (headerAvatar) headerAvatar.src = e.target.result;
            };
            reader.readAsDataURL(file);
        }
    }

    // Update profile display
    updateProfileDisplay(data) {
        const usernameDisplay = document.getElementById('usernameDisplay');
        const bioDisplay = document.getElementById('bioDisplay');
        const avatarPreview = document.getElementById('avatarPreview');
        const headerAvatar = document.querySelector('header img');

        if (usernameDisplay && data.username) usernameDisplay.textContent = data.username;
        if (bioDisplay && data.bio) bioDisplay.textContent = data.bio;
        if (avatarPreview && data.avatar) avatarPreview.src = data.avatar;
        if (headerAvatar && data.avatar) headerAvatar.src = data.avatar;
    }

    // Cleanup method remains unchanged...
}

// Pages object remains unchanged

// Game screen demo
class UIManager {
  static updateProfileDisplay(data, stats) {
    document.getElementById('username').value = data.username || '';
    document.getElementById('avatar').value = data.avatar || '';
    document.getElementById('bio').value = data.bio || '';
    document.getElementById('externalLinks').value = data.external_links?.join(', ') || '';
    const favGames = document.getElementById('favoriteGames');
    favGames.innerHTML = data.favorite_games?.slice(0, 5).map(poster => `<img src="${poster}" width="50">`).join('') || '';
    const currPlaying = document.getElementById('currentlyPlaying');
    currPlaying.innerHTML = data.currently_playing?.slice(0, 3).map(title => `<span>${title}</span>`).join(', ') || '';

    document.getElementById('followerCount').textContent = stats.follower_count || 0;
    document.getElementById('followingCount').textContent = stats.following_count || 0;
    document.getElementById('loggedGamesCount').textContent = stats.logged_games_count || 0;
    document.getElementById('listsCount').textContent = stats.lists_count || 0;
    // Add logic for recent logs, reviews, lists (assume IDs exist)
  }

  static addLoggedGamesToPage(games, loadMore = false) {
    const container = document.getElementById('loggedGamesGrid');
    if (!loadMore) container.innerHTML = '';
    games.forEach(game => {
      const card = document.createElement('div');
      card.className = 'game-card';
      card.innerHTML = `<h3>${game.title}</h3><div class="rating">${'★'.repeat(game.rating || 0)}</div>`;
      container.appendChild(card);
    });
    this.showLoadMore(games.length >= 10); // Assume 10 items per page
  }

  static showLoadMore(show) {
    const button = document.getElementById('loadMoreGames');
    if (button) button.style.display = show ? 'block' : 'none';
  }

  static collectProfileData() {
    return {
      username: document.getElementById('username').value,
      avatar: document.getElementById('avatar').value,
      bio: document.getElementById('bio').value,
      external_links: document.getElementById('externalLinks').value.split(',').map(l => l.trim()).filter(l => l),
      favorite_games: Array.from(document.querySelectorAll('#favoriteGames img')).map(img => img.src),
      currently_playing: document.getElementById('currentlyPlaying').textContent.split(',').map(t => t.trim())
    };
  }

  static showNotification(message) {
    const notification = document.createElement('div');
    notification.textContent = message;
    notification.style.cssText = 'position: fixed; top: 10px; left: 50%; transform: translateX(-50%); background: #4a90e2; color: white; padding: 10px; border-radius: 5px; z-index: 1000';
    document.body.appendChild(notification);
    setTimeout(() => notification.remove(), 3000);
  }

  static showLoading() {
    let loading = document.getElementById('loading');
    if (!loading) {
      loading = document.createElement('div');
      loading.id = 'loading';
      loading.textContent = 'Loading...';
      loading.style.cssText = 'position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: rgba(0,0,0,0.8); color: white; padding: 20px; border-radius: 5px; z-index: 1000';
      document.body.appendChild(loading);
    }
  }

  static hideLoading() {
    const loading = document.getElementById('loading');
    if (loading) loading.remove();
  }
}

// Reviews screen demo
class UIManager {
    static renderReviews(reviews) {
        const reviewsList = document.getElementById('reviewsList');
        const noResults = document.getElementById('noResults');
        const loadMoreSection = document.getElementById('loadMoreSection');

        if (reviews.length === 0) {
            reviewsList.innerHTML = '';
            noResults.classList.remove('hidden');
            loadMoreSection.classList.add('hidden');
            return;
        }

        noResults.classList.add('hidden');
        reviewsList.innerHTML = reviews.map(review => createReviewHTML(review)).join('');
        addGameClickEvents(); // Re-attach event listeners

        if (reviews.length >= filteredReviews.length) {
            loadMoreSection.classList.add('hidden');
        } else {
            loadMoreSection.classList.remove('hidden');
        }
    }

    static updateReviewStats(total) {
        const totalReviews = document.getElementById('totalReviews');
        totalReviews.textContent = total;
    }

    static updateFilterStats() {
        const showingCount = document.getElementById('showingCount');
        const totalCount = document.getElementById('totalCount');
        showingCount.textContent = displayedReviews.length;
        totalCount.textContent = filteredReviews.length;
    }

    static showNoResults() {
        const noResults = document.getElementById('noResults');
        noResults.classList.remove('hidden');
    }

    static showLoading() {
        const loadingIcon = document.getElementById('loadingIcon');
        const loadMoreText = document.getElementById('loadMoreText');
        loadingIcon.classList.add('active');
        loadMoreText.style.display = 'none';
    }

    static hideLoading() {
        const loadingIcon = document.getElementById('loadingIcon');
        const loadMoreText = document.getElementById('loadMoreText');
        loadingIcon.classList.remove('active');
        loadMoreText.style.display = 'inline';
    }

    static openModal(reviewData) {
        const modal = document.getElementById('addReviewModal');
        const gameTitle = document.getElementById('gameTitle');
        const ratingValue = document.getElementById('ratingValue');
        const reviewText = document.getElementById('reviewText');
        const reviewTags = document.getElementById('reviewTags');

        if (reviewData) {
            gameTitle.value = reviewData.title || '';
            ratingValue.value = reviewData.rating || 0;
            reviewText.value = reviewData.content || '';
            reviewTags.value = reviewData.tags ? reviewData.tags.join(', ') : '';
            highlightStars(reviewData.rating || 0);
        } else {
            reviewForm.reset();
            resetModalStarRating();
        }

        modal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }

    static closeModal() {
        const modal = document.getElementById('addReviewModal');
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
        reviewForm.reset();
        resetModalStarRating();
        reviewForm.onsubmit = handleAddReview; // Reset to default submission
    }

    static updateLikeCount(reviewId, likes) {
        const likeCount = document.querySelector(`[data-review-id="${reviewId}"] .like-count`);
        if (likeCount) likeCount.textContent = `${likes} likes`;
    }

    static showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `fixed top-4 right-4 px-6 py-3 rounded-lg text-white z-50 transition-all duration-300 transform translate-x-full`;

        switch (type) {
            case 'success': notification.classList.add('bg-green-600'); break;
            case 'error': notification.classList.add('bg-red-600'); break;
            case 'warning': notification.classList.add('bg-yellow-600'); break;
            default: notification.classList.add('bg-blue-600');
        }

        notification.innerHTML = `
            <div class="flex items-center gap-2">
                <i class="fas fa-${type === 'success' ? 'check' : type === 'error' ? 'times' : type === 'warning' ? 'exclamation' : 'info'}-circle"></i>
                <span>${message}</span>
            </div>
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.classList.remove('translate-x-full');
        }, 100);

        setTimeout(() => {
            notification.classList.add('translate-x-full');
            setTimeout(() => {
                if (notification.parentNode) notification.parentNode.removeChild(notification);
            }, 300);
        }, 3000);
    }
}

// Reusable functions from HTML (moved here for modularity)
function createReviewHTML(review) {
    const stars = '★'.repeat(review.rating) + '☆'.repeat(5 - review.rating);
    const formattedDate = new Date(review.date).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    return `
        <div class="bg-gray-800 rounded-lg p-6 review-card fade-in" data-review-id="${review.id}">
            <div class="flex gap-6">
                <div class="flex-shrink-0">
                    <img src="${review.poster}" alt="${review.title}" class="w-20 h-28 object-cover rounded-lg shadow-lg cursor-pointer hover:opacity-80 transition-opacity game-poster">
                </div>
                <div class="flex-1">
                    <div class="flex items-center justify-between mb-3">
                        <div>
                            <h3 class="text-xl font-bold text-white mb-1 cursor-pointer hover:text-blue-400 transition-colors game-title">${review.title}</h3>
                            <div class="flex items-center gap-2 text-green-400 text-lg">${stars}</div>
                        </div>
                        <div class="text-right">
                            <p class="text-gray-400 text-sm">${formattedDate}</p>
                            <div class="flex items-center gap-1 mt-1">
                                <button class="like-btn flex items-center gap-1 hover:text-red-400 transition-colors" data-review-id="${review.id}">
                                    <i class="fas fa-heart text-red-500 text-sm"></i>
                                    <span class="text-gray-400 text-sm like-count">${review.likes} likes</span>
                                </button>
                            </div>
                        </div>
                    </div>
                    <div class="text-gray-300 leading-relaxed">
                        ${review.content.split('\n').map(paragraph => `<p class="mb-3 last:mb-0">${paragraph}</p>`).join('')}
                    </div>
                    <div class="flex flex-wrap gap-2 mt-4">
                        ${review.tags.map(tag => `<span class="bg-blue-600 text-xs px-2 py-1 rounded-full cursor-pointer hover:bg-blue-700 transition-colors" onclick="searchByTag('${tag}')">#${tag}</span>`).join('')}
                    </div>
                    <div class="flex gap-2 mt-4">
                        <button class="edit-review-btn bg-yellow-600 hover:bg-yellow-700 px-3 py-1 rounded text-xs transition-colors" data-review-id="${review.id}">
                            <i class="fas fa-edit mr-1"></i>Edit
                        </button>
                        <button class="delete-review-btn bg-red-600 hover:bg-red-700 px-3 py-1 rounded text-xs transition-colors" data-review-id="${review.id}">
                            <i class="fas fa-trash mr-1"></i>Delete
                        </button>
                        <button class="share-review-btn bg-gray-600 hover:bg-gray-700 px-3 py-1 rounded text-xs transition-colors" data-review-id="${review.id}">
                            <i class="fas fa-share mr-1"></i>Share
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
}

function addGameClickEvents() {
    document.querySelectorAll('.game-title, .game-poster').forEach(element => {
        element.addEventListener('click', function() {
            const reviewCard = this.closest('.review-card');
            const reviewId = reviewCard.dataset.reviewId;
            const review = reviews.find(r => r.id == reviewId);
            navigateToGameDetail(review.title);
        });
    });
}

function navigateToGameDetail(gameTitle) {
    console.log(`Navigate to game detail page for: ${gameTitle}`);
    alert(`Navigating to ${gameTitle} detail page...`);
}

function showSearchResults() {
    if (searchInput.value.trim()) {
        const results = reviews.filter(review => review.title.toLowerCase().includes(searchInput.value.toLowerCase())).slice(0, 5);
        if (results.length > 0) {
            searchResults.innerHTML = results.map(review => `<div class="p-2 hover:bg-gray-600 cursor-pointer transition-colors" onclick="selectSearchResult('${review.title}')"><div class="flex items-center gap-2"><img src="${review.poster}" alt="${review.title}" class="w-8 h-10 object-cover rounded"><span>${review.title}</span></div></div>`).join('');
            searchResults.classList.remove('hidden');
        }
    }
}

function hideSearchResults() {
    setTimeout(() => searchResults.classList.add('hidden'), 200);
}

function selectSearchResult(title) {
    searchInput.value = title;
    hideSearchResults();
    const filters = { genre: genreFilter.value, platform: platformFilter.value, search: title.toLowerCase() };
    Handle.handleFilterReviews(filters);
}

function searchByTag(tag) {
    searchInput.value = tag;
    const filters = { genre: genreFilter.value, platform: platformFilter.value, search: tag.toLowerCase() };
    Handle.handleFilterReviews(filters);
}

function shareReview(reviewId) {
    const review = reviews.find(r => r.id === reviewId);
    if (review) {
        const shareText = `Check out my review of ${review.title}! I gave it ${review.rating}/5 stars. ${review.content.substring(0, 100)}...`;
        if (navigator.share) {
            navigator.share({ title: `${review.title} Review`, text: shareText, url: window.location.href });
        } else {
            navigator.clipboard.writeText(shareText).then(() => UI.showNotification('Review copied to clipboard!', 'success'));
        }
    }
}

function setupModalStarRating() {
    const stars = document.querySelectorAll('#modalStarRating .star');
    const ratingValue = document.getElementById('ratingValue');
    stars.forEach((star, index) => {
        star.addEventListener('mouseover', () => highlightStars(index + 1));
        star.addEventListener('mouseout', () => highlightStars(parseInt(ratingValue.value)));
        star.addEventListener('click', () => {
            const rating = index + 1;
            ratingValue.value = rating;
            highlightStars(rating);
        });
    });
}

function highlightStars(rating) {
    const stars = document.querySelectorAll('#modalStarRating .star');
    stars.forEach((star, index) => {
        if (index < rating) {
            star.classList.remove('text-gray-600');
            star.classList.add('text-yellow-400');
        } else {
            star.classList.remove('text-yellow-400');
            star.classList.add('text-gray-600');
        }
    });
}

function resetModalStarRating() {
    document.getElementById('ratingValue').value = '0';
    highlightStars(0);
}

const UI = UIManager;