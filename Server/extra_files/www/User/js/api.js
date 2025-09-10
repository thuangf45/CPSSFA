// === Base API ===
class API {
  static baseUrl = ''; // Override ở class con

  // Hàm để gửi request đến API
  // method: 'GET', 'POST', 'PUT', 'DELETE'
  async request(method, path, options = {}) {
    const url = this.constructor.baseUrl + path; // url của API

    const headers = options.headers || {}; // Tạo headers từ options
    const token = API.getToken();  // Lấy token từ localStorage

    if (token) {
      headers['X_Token_Authorization'] = token; // Gửi token nếu có
    }

    // Ngrok warning
    headers['ngrok-skip-browser-warning'] = 'true';

    options.headers = headers; // thêm dòng này để đảm bảo headers có token

    // fetch để gửi request và nhận response
    const res = await fetch(url, {
      ...options, // Tham số bổ sung từ options
      method, // Phương thức HTTP (GET, POST, PUT, DELETE)
      credentials: 'include' // Gửi cookie (sessionId) kèm theo request  (nếu server dùng cookie, vẫn giữ)
    });

    // === Nhận token mới nếu có ===
    const newToken = res.headers.get('X_Token_Authorization');
    if (newToken) { // Nếu có header X_Token_Authorization trong response
      API.setToken(newToken); // Lưu lại token mới
    }

    const contentType = res.headers.get('content-type') || ''; // Lấy header Content-Type
    const isJson = contentType.includes('application/json'); // Kiểm tra xem response có phải JSON không
    const data = isJson ? await res.json() : await res.text(); // Đọc dữ liệu từ response

    return { ok: res.ok, status: res.status, data }; // Trả về đối tượng chứa thông tin response
  }

  // Hàm để xây dựng query string từ đối tượng params
  // Ví dụ: buildQuery({ key1: 'value1', key2: 'value2' }) => '?key1=value1&key2=value2'
  static buildQuery(params = {}) {
    const query = Object.entries(params)
      .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`)
      .join('&');
    return query ? `?${query}` : '';
  }
  static buildPathWithQuery(path = '', params = {}) {
    const query = this.buildQuery(params);
    const prefix = path.startsWith('/') ? path : `/${path}`;
    return query ? `${prefix}${query}` : `${prefix}`;
  }
  // Hàm để lấy token từ localStorage
  static getToken() {
    return localStorage.getItem('authToken');
  }

  // Hàm để lưu token vào localStorage
  static setToken(token) {
    localStorage.setItem('authToken', token);
  }

  // Hàm GET để lấy dữ liệu từ API
  GET(path, options = {}) {
    return this.request('GET', path, options);
  }

  // Hàm POST để gửi dữ liệu đến API
  POST(path, options = {}) {
    return this.request('POST', path, options);
  }

  // Hàm PUT để cập nhật dữ liệu trên API
  PUT(path, options = {}) {
    return this.request('PUT', path, options);
  }

  // Hàm DELETE để xóa dữ liệu trên API
  DELETE(path, options = {}) {
    return this.request('DELETE', path, options);
  }
}

export class APIAuth extends API {
  static baseUrl = '/api/auth';

  // kiểm tra tài khoản mật khẩu trả về okay 
  async PostLogin(username, password) {
    if (!username || !password) {
      alert('Username and password are required!');
      return;
    }
    const payload = { username, password }
    const query = API.buildPathWithQuery('login');

    const res = await this.POST(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async PostLogout() {
    const query = API.buildPathWithQuery('logout');
    const res = await this.POST(query, {
      headers: { 'Content-Type': 'application/json' },
      body: '{}' // hoặc có thể bỏ hẳn nếu server không cần body
    });

    return res;
  }

  async PostForgetPassword(email) {
    if (!email) {
      alert('Email is required!');
      return;
    }
    const payload = { email }
    const query = API.buildPathWithQuery('forgetpassword');
    const res = await this.POST(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

}

// API để tương tác với user
export class APIUser extends API {
  static baseUrl = '/api/user';

  async GetUser(userId = null, username = null) {

    if (userId || username) {
      const query = API.buildQuery({ userId, username });

      const res = await this.GET(query);
      return res;
    }
    else {
      const query = API.buildQuery({});
      const res = await this.GET(query);
      return res;
    }

  }

  async GetUserFollower(userId = null, page = 1, limit = 10) {
    const query = API.buildPathWithQuery('follower', { page, limit, userId })

    const res = await this.GET(query);
    return res;
  }

  async GetUserFollowing(userId = null, page = 1, limit = 10) {
    const query = API.buildPathWithQuery('following', { page, limit, userId })

    const res = await this.GET(query);
    return res;
  }

  async PostUserFollow(target) {
    const query = API.buildPathWithQuery('follow');
    const payload = { target };

    const res = await this.POST(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async PostUser(username, password, email) {
    const payload = { username, password, email };

    const res = await this.POST('', {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }


  async PutUserEmail(email) {
    const payload = { email }
    const query = API.buildPathWithQuery('email');

    const res = await this.PUT(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async PutUserAvatar(avatar) {
    const payload = { avatar }
    const query = API.buildPathWithQuery('avatar');

    // Không set Content-Type, fetch tự set multipart/form-data
    const res = await this.PUT(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }
  async PutUserUsername(username = '') {
    const payload = { username }
    const query = API.buildPathWithQuery('username');

    const res = await this.PUT(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }
  async PutUserPassword(oldpassword, newpassword) {
    const payload = { oldpassword, newpassword }
    const query = API.buildPathWithQuery('password');

    const res = await this.PUT(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async DeleteUser(password) {
    const payload = { password }
    const query = API.buildQuery({});

    const res = await this.DELETE(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async DeleteUserFollow(target) {
    const query = API.buildPathWithQuery('follow');
    const payload = { target };

    const res = await this.DELETE(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

}
// API để tương tác với review
export class ReviewAPI extends API {
  static baseUrl = '/api/review';

  async GetReview(reviewId) {
    const query = API.buildQuery({ reviewId });

    const res = await this.GET(query);

    return res;
  }

  async GetReviewByUser(userId = null) {
    const query = API.buildPathWithQuery('user', { userId });

    const res = await this.GET(query);

    return res;
  }

  async GetReviewByGame(gameId) {
    const query = API.buildPathWithQuery('game', { gameId });

    const res = await this.GET(query);

    return res;
  }

  async PostReview(gameId, content, rating) {
    const payload = { gameId, content, rating };

    const res = await this.POST('', {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async PutReview(reviewId, gameId, content, rating) {
    const payload = { reviewId, gameId, content, rating };

    const res = await this.PUT('', {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async DeleteReview(reviewId, gameId) {
    const payload = { reviewId, gameId };
    const query = API.buildQuery({});

    const res = await this.DELETE(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }
}

// API để tương tác với game
export class GameAPI extends API {
  static baseUrl = '/api/game';

  async GetGame(gameId) {
    const query = API.buildQuery({ gameId });

    const res = await this.GET(query);

    return res;
  }

  async GetGameByUser(userId = null) {
    const query = API.buildPathWithQuery('user', { userId });

    const res = await this.GET(query);

    return res;
  }
  
  async GetGamePagination(page = 1, limit = 10) {
    const query = API.buildPathWithQuery('pagination', { page, limit });

    const res = await this.GET(query);

    return res;
  }
}

// API để tương tác với Comment 
export class CommentAPI extends API {
  static baseUrl = '/api/comment';

  // Lấy comment 
  async GetComment(commentId) {
    const query = API.buildQuery({ commentId });

    const res = await this.GET(query);

    return res;
  }


  //Commment cuar ca nhana
  async GetCommentByUser(userId = null) {
    const query = API.buildPathWithQuery('user', { userId });

    const res = await this.GET(query);

    return res;
  }


  //Comment cua review
  async GetCommentByReview(reviewId) {
    const query = API.buildPathWithQuery('review', { reviewId });

    const res = await this.GET(query);

    return res;
  }

  // Tạo mới comment 
  async PostComment(content, reviewId) {
    const payload = { reviewId, content };

    const res = await this.POST('', {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  // Cập nhật nội dung comment 
  async PutComment(commentId, reviewId, content) {
    const payload = { commentId, reviewId, content };

    const res = await this.PUT('', {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  // Xoá comment theo 
  async DeleteComment(commentId, reviewId) {
    const payload = { commentId, reviewId };

    const res = await this.DELETE('', {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }
}

// API để tương tác với Reaction 
export class ReactionAPI extends API {
  static baseUrl = '/api/reaction';

  // Lấy reaction 
  async GetReactionByComment(commentId) {
    const query = API.buildPathWithQuery('comment', { commentId });

    const res = await this.GET(query);

    return res;
  }

  async GetReactionByUser(userId = null) {
    const query = API.buildPathWithQuery('user', { userId });

    const res = await this.GET(query);

    return res;
  }

  async GetReactionByReview(reviewId) {
    const query = API.buildPathWithQuery('review', { reviewId });

    const res = await this.GET(query);

    return res;
  }

  // Tạo mới reaction 
  async PostReactionForComment(reactionType, commentId) {
    const query = API.buildPathWithQuery('comment');
    const payload = { commentId, reactionType };

    const res = await this.POST(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async PostReactionForReview(reactionType, reviewId) {
    const query = API.buildPathWithQuery('review');
    const payload = { reviewId, reactionType };

    const res = await this.POST(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  // Cập nhật nội dung reaction 
  async PutReactionForComment(reactionId, reactionType, commentId) {
    const query = API.buildPathWithQuery('comment');
    const payload = { reactionId, commentId, reactionType };

    const res = await this.PUT(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async PutReactionForReview(reactionId, reactionType, reviewId) {
    const query = API.buildPathWithQuery('review');
    const payload = { reactionId, reviewId, reactionType };

    const res = await this.PUT(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  // Xoá reaction 
  async DeleteReactionOfComment(commentId, reactionId) {
    const payload = { commentId, reactionId };
    const query = API.buildPathWithQuery('comment');

    const res = await this.DELETE(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

  async DeleteReactionOfReview(reviewId, reactionId) {
    const payload = { reviewId, reactionId };
    const query = API.buildPathWithQuery('review');

    const res = await this.DELETE(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    return res;
  }

}


// API để tương tác với cache (Admin)
export class CacheAPI extends API {
  static baseUrl = '/api/cache';

  async GetCache(key) {
    const query = API.buildQuery({ key });

    const res = await this.GET(query);

    return res;
  }


  async PostCache(key, value) {
    if (!key) return alert('Key is required');

    const query = API.buildQuery({ key });

    const res = await this.POST(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(value)
    });

    return res;
  }

  async PutCache(key, value) {
    if (!key) return alert('Key is required');

    const query = API.buildQuery({ key });

    const res = await this.PUT(query, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(value)
    });

    return res;
  }

  async DeleteCache(key) {
    if (!key) return alert('Key is required');

    const query = API.buildQuery({ key });

    const res = await this.DELETE(query);

    return res;
  }
}
