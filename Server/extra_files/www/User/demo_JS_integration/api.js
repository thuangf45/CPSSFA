// Profile screen demo
// Add this to the existing APIUser class in api.js
export class APIUser extends API {
  static baseUrl = '/api/user';

  // Update user profile (password, email, avatar, username)
  async UpdateProfile(username, password, email, avatar) {
    const formData = new FormData();
    formData.append('username', username || '');
    formData.append('password', password || '');
    formData.append('email', email || '');
    if (avatar) {
      formData.append('avatar', avatar);
    }

    const res = await this.POST('/profile', {
      headers: {
        'Accept': 'application/json'
      },
      body: formData
    });

    return res;
  }

  // Get user profile
  async GetProfile() {
    const res = await this.GET('/profile');
    return res;
  }
}

// Game screen demo
class GameAPI {
  static async GetProfileGames(userId) {
    const token = API.getToken();
    return await API.fetchData(`/api/games/profile/${userId}`, {
      headers: { 'X_Token_Authorization': `Bearer ${token}` }
    });
  }

  static async GetStats(userId) {
    const token = API.getToken();
    return await API.fetchData(`/api/games/stats/${userId}`, {
      headers: { 'X_Token_Authorization': `Bearer ${token}` }
    });
  }

  static async GetLoggedGames(userId, filter = {}, sort = 'Date Logged', reviewedOnly = false) {
    const token = API.getToken();
    const query = API.buildQuery({ ...filter, sort, reviewedOnly });
    return await API.fetchData(`/api/games/logged/${userId}${query}`, {
      headers: { 'X_Token_Authorization': `Bearer ${token}` }
    });
  }

  static async PutProfileGames(userId, data) {
    const token = API.getToken();
    return await API.fetchData(`/api/games/profile/${userId}`, {
      method: 'PUT',
      headers: { 'X_Token_Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
  }
}

// Ensure API class has these methods
API.buildQuery = function(params) {
  const query = Object.keys(params)
    .filter(key => params[key] !== undefined && params[key] !== '')
    .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(params[key])}`)
    .join('&');
  return query ? `?${query}` : '';
};

API.fetchData = async function(url, options = {}) {
  const response = await fetch(url, options);
  if (!response.ok) throw new Error('Network response was not ok');
  return response.json();
};

API.setToken = function(token) { localStorage.setItem('token', token); };
API.getToken = function() { return localStorage.getItem('token'); };

// Reviews screen demo
class API {
    static getToken() {
        // Mock token for now; replace with actual token retrieval
        return "mock-token-123";
    }

    static async fetchData(url, options = {}) {
        const token = API.getToken();
        const headers = { ...options.headers, 'X_Token_Authorization': `Bearer ${token}` };
        const response = await fetch(url, { ...options, headers });
        if (!response.ok) throw new Error("API request failed");
        return response.json();
    }
}

class GameAPI {
    static async GetReviews(userId) {
        return await API.fetchData(`/api/reviews/${userId}`);
    }

    static async GetFilteredReviews(userId, filters, sort) {
        const query = new URLSearchParams({
            ...filters,
            sort: sort
        }).toString();
        return await API.fetchData(`/api/reviews/filtered/${userId}?${query}`);
    }

    static async PostReview(userId, reviewData) {
        return await API.fetchData(`/api/reviews/${userId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(reviewData)
        });
    }

    static async PutReview(userId, reviewId, reviewData) {
        return await API.fetchData(`/api/reviews/${userId}/${reviewId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(reviewData)
        });
    }

    static async DeleteReview(userId, reviewId) {
        return await API.fetchData(`/api/reviews/${userId}/${reviewId}`, {
            method: 'DELETE'
        });
    }

    static async PostReviewLike(userId, reviewId) {
        return await API.fetchData(`/api/reviews/like/${userId}/${reviewId}`, {
            method: 'POST'
        });
    }
}
