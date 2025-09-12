import { APIAuth, APIUser, GameAPI, ReviewAPI, ReactionAPI, CommentAPI } from './api.js';
import { View } from './view.js';
import { uploadImage } from './externalapi.js';
import { Model, Pages } from './model.js';


export class Controller {
    // ====== API method =======

    // ======= User =========
    static async Login(username, password) {
        if (!username || !password) {
            View.showWarning("Vui lòng nhập đầy đủ thông tin!");
            return false;
        }

        const api = new APIAuth();
        View.showLoading();

        const res = await api.PostLogin(username, password);
        View.hideLoading();


        if (!res.ok) {
            View.showWarning("Tài khoản hoặc mật khẩu không đúng!");
            return false;
        }

        View.showSuccess("Đăng nhập thành công!");

        View.goTo(Pages.Page.HOME);
        return true;
    }

    static async Register(username, password, email) {
        if (!username || !password || !email) {
            View.showWarning("VView lòng nhập đầy đủ thông tin!");
            return false;
        }

        const api = new APIUser();
        View.showLoading();
        const res = await api.PostUser(username, password, email);
        View.hideLoading();

        if (!res.ok) {
            View.showWarning("Thông tin đăng ký không hợp lệ");
            return false;
        }

        View.showSuccess("Đăng ký thành công");
        View.goTo(Pages.Page.PROFILE);
        return true;
    }

    static async ChangeAvatar(file, avatar) {
        if (!file) {
            View.showWarning("Vuilòng chọn ảnh");
            return false;
        }

        if (!file.type.startsWith('image/')) {
            View.showWarning("Vui lòng chọn đúng định dạng ảnh");
            return false;
        }

        if (file.size > 2 * 1024 * 1024) { // 2MB
            View.showWarning("Ảnh vượt quá dung lượng cho phép (2MB)");
            return false;
        }

        View.showLoading();
        const imageUrl = await uploadImage(file);
        const api = new APIUser();
        const res = await api.PutUserAvatar(imageUrl);
        View.hideLoading();

        if (res.status === 401) {
            Model.deleteAuthToken();
            View.goTo(Pages.Page.AUTH);
            return false;
        }

        if (!res.ok) {
            View.showWarning("Đổi avatar thất bại!")
            return false;
        }

        avatar.src = imageUrl;
        View.showSuccess("Đổi avatar thành công!")
        return true;

    }

    static async ChangePassword(oldpassword, newPassword) {
        const api = new APIUser();
        View.showLoading();

        const res = await api.PutUserPassword(oldpassword, newPassword)
        View.hideLoading();

        if (res.status === 401) {
            Model.deleteAuthToken();
            View.goTo(Pages.Page.AUTH);
            return false;
        }

        if (!res.ok) {
            View.showWarning("Đổi password thất bại!")
            return false;
        }

        View.showSuccess("Đổi password thành công!")
        return true;
    }

    static async ChangeUsername(username) {
        const api = new APIUser();
        View.showLoading();

        const res = await api.PutUserUsername(username)
        View.hideLoading();

        if (res.status === 401) {
            Model.deleteAuthToken();
            View.goTo(Pages.Page.AUTH);
            return false;
        }

        if (!res.ok) {
            View.showWarning("Đổi username thất bại!")
            return false;
        }

        View.showSuccess("Đổi username thành công!")
        return true;
    }

    static async ChangeEmail(email) {
        const api = new APIUser();
        View.showLoading();

        const res = await api.PutUserEmail(email)
        View.hideLoading();

        if (res.status === 401) {
            Model.deleteAuthToken();
            View.goTo(Pages.Page.AUTH);
            return false;
        }

        if (!res.ok) {
            View.showWarning("Đổi email thất bại!")
            return false;
        }
        View.showSuccess("Đổi email thành công!")
        return true;
    }


    static async Forgot(email) {
        if (email == null) {
            View.showWarning("Vui lòng nhập đầy đủ thông tin!")
            return false;
        }
        const api = new APIAuth();
        View.showLoading();

        const res = await api.PostForgetPassword(email);
        View.hideLoading();

        if (!res.ok) {
            View.showWarning("Email chưa được đăng ký")
            return false;
        }

        View.showSuccess("Gửi mã thiết lập lại mật khẩu thành công")
        View.goTo(Pages.Page.AUTH);
        return true;
    }

    static async Logout() {
        const api = new APIAuth();
        View.showLoading();

        const res = await api.PostLogout()
        View.hideLoading();

        if (res.status === 401) {
            Model.deleteAuthToken();
            View.goTo(Pages.Page.AUTH);
            return true;
        }
        if (!res.ok) {
            View.showWarning("Đăng xuất không thành công")
            return false;
        }

        View.showSuccess("Đăng xuất thành công")
        Model.deleteAuthToken();
        View.goTo(Pages.Page.INDEX)
        return true;
    }

    static async DeleteAcc(password) {
        const api = new APIUser();
        View.showLoading();

        const res = await api.DeleteUser(password)
        View.hideLoading();

        if (res.status === 401) {
            Model.deleteAuthToken();
            View.goTo(Pages.Page.AUTH);
            return;
        }
        if (!res.ok) {
            View.showWarning("Xóa tài khoản không thành công")
            return false;
        }

        View.showSuccess("Xóa tài khoản thành công")
        Model.deleteAuthToken();
        View.goTo(Pages.Page.INDEX)
        return true;
    }

    static async ShowUserInfo() {
        const api = new APIUser();

        View.showLoading();
        const userInfo = Model.getLocalStorageJSON('userInfo');

        // Xác suất fetch lại (30% chẳng hạn)
        const shouldRefetch = Math.random() < 0.15;

        if (!userInfo || shouldRefetch) {
            const res = await api.GetUser();
            View.hideLoading();

            if (!res.ok) {
                View.showUserInfo(userInfo);
                return false;
            }

            Model.setLocalStorageJSON('userInfo', res.data);
            View.showUserInfo(res.data);
            return true;
        }

        View.hideLoading();
        View.showUserInfo(userInfo);
        return true;
    }


    static async ShowStat() {
        const api = new APIUser();
        View.showLoading();

        const res1 = await api.GetUserFollower();
        const res2 = await api.GetUserFollowing();

        if (!res1.ok || !res2.ok) {

        }

        View.updateStats()
        return true;

    }








    // ========= Game Review ==============
    //reaction section
    static async reactToReview(reactionType, reviewId, btnEl) {
        try {
            const api = new ReactionAPI();
            const res = await api.PostReactionForReview(reactionType, reviewId);

            if (res.success) {
                const countSpan = btnEl.querySelector("span");
                if (countSpan) {
                    let count = parseInt(countSpan.textContent) || 0;
                    countSpan.textContent = count + 1;
                }
            }
        } catch (err) {
            console.error("Error reacting to review:", err);
        }
    }

    static async reactToComment(reactionType, commentId, btnEl) {
        try {
            const api = new ReactionAPI();
            const res = await api.PostReactionForComment(reactionType, commentId);

            if (res.success) {
                // tăng count trong UI
                const countSpan = btnEl.querySelector("span");
                if (countSpan) {
                    let count = parseInt(countSpan.textContent) || 0;
                    countSpan.textContent = count + 1;
                }
            }
        } catch (err) {
            console.error("Error reacting to comment:", err);
        }
    }



    // ========= Game ===========
    static async GetGamePopular(gameData) {
        // Sort theo rating (giảm dần) và lấy 10 game
        const popularGames = [...gameData]
            .sort((a, b) => b.NumberReview - a.NumberReview) // sắp xếp theo thuộc tính
            .slice(0, 10); // lấy 10 phần tử đầu tiên

        View.showGamePopular(popularGames)
        return true;
    }

    static async GetGameBest(gameData) {
        // Sort theo rating (giảm dần) và lấy 10 game
        const bestGames = [...gameData]
            .sort((a, b) => b.AvgRating - a.AvgRating) // sắp xếp theo thuộc tính
            .slice(0, 10); // lấy 10 phần tử đầu tiên

        View.showGameBest(bestGames)
        return true;
    }

    static async GetGamePagination(gameData, page, limit) {
        // Tính index bắt đầu và kết thúc
        const startIndex = (page - 1) * limit; // ví dụ page=1, limit=10 => start=0
        const endIndex = page * limit;         // ví dụ page=1, limit=10 => end=10

        // Cắt mảng dữ liệu
        const paginatedData = gameData.slice(startIndex, endIndex);

        // Truyền cho View để render
        View.showGamePagination(paginatedData);
        return true;
    }

    static async ShowGameDetail(gameId) {
        View.showLoading();

        const gameData = Model.getLocalStorageJSON('gameData');

        if (gameData) {
            for (let i = 0; i < gameData.length; i++) {
                if (gameData[i].GameId == gameId) {
                    View.showGameDetail(gameData[i]);
                    View.hideLoading();
                    return true;
                }
            }
        }

        const api = new GameAPI();
        const res = await api.GetGame(gameId);

        if (!res.ok) {
            View.showError('Có lỗi xảy ra!');
            return false;
        }

        View.showGameDetail(res.data);
        View.hideLoading();

        return true;

    }

    static async ShowReviews(gameId) {
        View.showLoading();
        const reviewOfGame = Model.getLocalStorageJSON(gameId);

        // Xác suất fetch lại (15% chẳng hạn)
        const shouldRefetch = Math.random() > 0.15;

        if (reviewOfGame && shouldRefetch) {
            View.renderReview(reviewOfGame);
            View.hideLoading();
            return true;
        }

        const api = new ReviewAPI();

        const res = await api.GetReviewByGame(gameId);

        if (!res.ok) {
            View.showError('Có lỗi xảy ra!');
            View.hideLoading();
            return false;
        }

        View.renderReview(res.data);
        Model.setLocalStorageJSON(gameId, res.data);
        View.hideLoading();
        return true;
    }

    static async ShowDetails(gameId) {
        View.showLoading();

        const gameData = Model.getLocalStorageJSON('gameData');


        // Xác suất fetch lại (15% chẳng hạn)
        const shouldRefetch = Math.random() > 0.15;

        if (gameData && shouldRefetch) {
            for (let i = 0; i < gameData.length; i++) {
                if (gameData[i].GameId == gameId) {
                    View.renderDetailsTab(gameData[i].Descriptions, gameData[i].Details);
                    View.hideLoading();
                    return true;
                }
            }
        }

        const api = new GameAPI();

        const res = await api.GetGame(gameId);


        if (!res.ok) {
            View.showError('Có lỗi xảy ra!');
            View.hideLoading();
            return false;
        }

        View.renderDetailsTab(res.data.Descriptions, res.data.Details);
        View.hideLoading();
        return true;
    }

    static async ShowGenre(gameId) {
        View.showLoading();

        const gameData = Model.getLocalStorageJSON('gameData');


        // Xác suất fetch lại (15% chẳng hạn)
        const shouldRefetch = Math.random() > 0.15;

        if (gameData && shouldRefetch) {
            for (let i = 0; i < gameData.length; i++) {
                if (gameData[i].GameId == gameId) {
                    View.renderGenresTab(gameData[i].Genre);
                    View.hideLoading();
                    return true;
                }
            }
        }

        const api = new GameAPI();

        const res = await api.GetGame(gameId);


        if (!res.ok) {
            View.showError('Có lỗi xảy ra!');
            View.hideLoading();
            return false;
        }

        View.renderGenresTab(gameData[i].Genre);
        View.hideLoading();
        return true;
    }

    static async ShowService(gameId) {
        View.showLoading();

        const gameData = Model.getLocalStorageJSON('gameData');

        // Xác suất fetch lại (15% chẳng hạn)
        const shouldRefetch = Math.random() > 0.15;

        if (gameData && shouldRefetch) {
            for (let i = 0; i < gameData.length; i++) {
                if (gameData[i].GameId == gameId) {
                    View.renderServicesTab(gameData[i].Services);
                    View.hideLoading();
                    return true;
                }
            }
        }

        const api = new GameAPI();

        const res = await api.GetGame(gameId);


        if (!res.ok) {
            View.showError('Có lỗi xảy ra!');
            View.hideLoading();
            return false;
        }

        View.renderServicesTab(gameData[i].Services);
        View.hideLoading();
        return true;
    }




    static async LoadHomeContent(page, limit) {
        let gameData = Model.getLocalStorageJSON('gameData');
        if (!gameData || gameData.length < 10) {
            const api = new GameAPI();
            View.showLoading();
            const res = await api.GetGamePagination(1, 200);

            if (!res.ok) {
                View.showWarning("Gặp lỗi khi lấy dữ liệu game")
                return false;
            }

            gameData = res.data;
            Model.setLocalStorageJSON('gameData', gameData);
            View.hideLoading();
        }

        Controller.GetGamePopular(gameData);
        Controller.GetGameBest(gameData);
        Controller.GetGamePagination(gameData, page, limit);

        const dataGame = Model.getLocalStorageJSON('gameData');
        const numberPage = dataGame.length / 10;
        View.updatePaginationUI(page, numberPage)
    }

    // ===========Review API===================
    static async GetReview() {
        const api = new ReviewAPI();
        View.showLoading();

        const res = await api.GetReviewByUser()
        View.hideLoading();

        if (!res.ok) {
            View.showWarning("Gặp lỗi khi lấy dữ liệu reviews")
            return false;
        }

        View.showReview(res.data)
        return true;

    }

    static async SaveReview(gameId, content, rating) {
        const api = new ReviewAPI();
        View.showLoading();

        const res = await api.PostReview(gameId, content, rating);
        View.hideLoading();

        if (res.status === 401) {
            Model.deleteAuthToken();
            View.goTo(Pages.Page.AUTH);
            return false;
        }

        if (!res.ok) {
            View.showWarning("Tạo review thất bại!");
            return false;
        }

        View.showSuccess("Tạo review thành công!");

        return true;
    }

    static async ShowReviewDetail(reviewId) {
        const api = new ReviewAPI();
        View.showLoading();

        const resR = await api.GetReview(reviewId);

        if (!resR.ok) {
            View.showError('Có lỗi xảy ra!');
            View.hideLoading();
            return false;
        }

        const reviewData = resR.data;

        let commentData = Model.getLocalStorageJSON(reviewData.ReviewId);

        // Xác suất fetch lại (15% chẳng hạn)
        const shouldRefetch = Math.random() > 0.15;

        if (commentData && shouldRefetch) {
            View.renderReviewCard(reviewData, commentData);
            View.hideLoading();
            return true;
        }

        const apiC = new CommentAPI();
        const resC = await apiC.GetCommentByReview(reviewId);
        
        commentData = resC.data;

        if (!resC.ok) {
            View.showError('Có lỗi xảy ra!');
            View.hideLoading();
            return true;
        }

        Model.setLocalStorageJSON(reviewId, commentData);
        View.renderReviewCard(reviewData, commentData);
        
        View.hideLoading();
        return true;
    }

    static closeReviewModal() {
        document.getElementById("reviewModal").classList.add("hidden");
    }


    //========= Game Detail =============
    // ======== Web client logic method
    static resetIdleTimer() {
        clearTimeout(Model.idleTimer);
        Model.idleTimer = setTimeout(() => {
            View.showWarning('Bạn đã quá thời gian chờ');
            Model.deleteAuthToken();
            View.goTo(Pages.Page.AUTH);
        }, Model.idleLimit);
    }

    static async LoadGamesContent(page, limit) {
        let gameData = Model.getLocalStorageJSON('gameData');
        if (!gameData || gameData.length < 10) {
            const api = new GameAPI();
            View.showLoading();
            const res = await api.GetGamePagination(1, 200);

            if (!res.ok) {
                View.showWarning("Gặp lỗi khi lấy dữ liệu game");
                View.hideLoading(); // Ensure loading is hidden on failure
                return false;
            }

            gameData = res.data;
            Model.setLocalStorageJSON('gameData', gameData);
            View.hideLoading();
        }

        const startIndex = (page - 1) * limit;
        const endIndex = page * limit;
        const paginatedData = gameData.slice(startIndex, endIndex);
        if (paginatedData.length === 0) {
            View.showWarning("No games to display for this page");
            View.hideLoading();
            return false;
        }
        View.showGameListPagination(paginatedData);
        return true;
    }
    

    static async ShowUserInfo() {
    const api = new APIUser();

    View.showLoading();
    const userInfo = Model.getLocalStorageJSON('userInfo');

    const shouldRefetch = Math.random() < 0.3;

    if (!userInfo || shouldRefetch) {
        const res = await api.GetUser();
        View.hideLoading();

        if (!res.ok) {
            View.showUserStats(userInfo || {});
            return false;
        }

        Model.setLocalStorageJSON('userInfo', res.data);
        View.showUserStats(res.data);
    } else {
        View.hideLoading();
        View.showUserStats(userInfo);
    }

    const followerRes = await api.GetUserFollower();
    const followingRes = await api.GetUserFollowing();
    if (followerRes.ok) View.showFollowers(followerRes.data || []);
    if (followingRes.ok) View.showFollowing(followingRes.data || []);

    const reviewRes = await api.GetReviewByUser();
    if (reviewRes.ok) {
        View.showHighestRatedGames(reviewRes.data || []);
        View.showRecentlyRatedGames(reviewRes.data || []);
    }

    return true;
}

static async SaveReview(gameId, content, rating) {
    const api = new ReviewAPI();
    View.showLoading();

    const res = await api.PostReview(gameId, content, rating);
    View.hideLoading();

    if (res.status === 401) {
        Model.deleteAuthToken();
        View.goTo(Pages.Page.AUTH);
        return false;
    }

    if (!res.ok) {
        View.showWarning("Tạo review thất bại!");
        return false;
    }

    View.showSuccess("Tạo review thành công!");
    // Refresh user info to include the new game rating
    await Controller.ShowUserInfo();
    return true;
}
}
