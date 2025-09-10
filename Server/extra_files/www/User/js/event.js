import { Controller } from './controller.js';
import { View } from './view.js';
import { Model, Pages } from './model.js';

View.init();

// Hàm tiện ích: chỉ thêm sự kiện nếu phần tử tồn tại
function listenIfExists(selector, event, handler) {
    const element = document.querySelector(selector);
    if (element) {
        element.addEventListener(event, handler);
    }
}

// Hàm tiện ích: Thêm listener vào window nếu tồn tại (luôn tồn tại)
function listenWindow(event, handler) {
    if (typeof handler === 'function') {
        window.addEventListener(event, handler);
    }
}



document.addEventListener('DOMContentLoaded', () => {
    // ========== Element event ===========
    // ========== Auth ===========
    // Đăng nhập
    listenIfExists('#button-auth', 'click', () => {
        const username = document.getElementById('loginUsername')?.value || '';
        const password = document.getElementById('loginPassword')?.value || '';
        Controller.Login(username, password);
    });


    // ========== Register ===========
    // Đăng ký
    listenIfExists('#signupBtn', 'click', () => {
        const username = document.getElementById('signupUsername')?.value || '';
        const password = document.getElementById('signupPassword')?.value || '';
        const email = document.getElementById('signupEmail')?.value || '';
        Controller.Register(username, password, email);
    });

    // Quên mật khẩu
    listenIfExists('#forgotBtn', 'click', () => {
        const email = prompt('Please enter your email address:');
        if (email) {
            Controller.Forgot(email)
        }

    });


    // ========= Profile ===========

    // Lắng nghe sự kiện "change" để xem trước ảnh
    listenIfExists('#editAvatar', 'change', (e) => {
        const file = e.target.files[0];
        const preview = document.getElementById('avatarPreview');

        if (file && file.type.startsWith('image/')) {
            // Gán file vào biến toàn cục để có thể sử dụng sau
            Model.selectedFile = file;

            // Tạo URL xem trước và hiển thị
            const reader = new FileReader();
            reader.onload = (e) => {
                preview.src = e.target.result;
            };
            reader.readAsDataURL(file);
        } else {
            // Nếu file không hợp lệ, reset biến và ảnh preview
            Model.selectedFile = null;
            // Bạn có thể reset preview.src về ảnh avatar cũ ở đây
        }
    });
    listenIfExists('#avatarPreview', 'click', () => {
        const editAvatar = document.getElementById('editAvatar');
        editAvatar.click();
    });

    // xóa tài khoản 
    listenIfExists('#deleteAccount', 'click', () => {
        const passwordInput = document.getElementById('deletePassword'); // 
        const password = passwordInput.value;

        if (!password) {
            View.showWarning("Vui lòng nhập mật khẩu");
            return;
        }

        if (confirm("Bạn có chắc chắn muốn xóa tài khoản? Hành động này không thể hoàn tác.")) {
            Controller.DeleteAcc(password);
        }
    });

    // Ví dụ thêm: Đăng xuất
    listenIfExists('#button-logout', 'click', () => {
        Controller.Logout();
    });

    // EDIT PAGE
    listenIfExists('#editButton', 'click', () => {
        const editMode = document.getElementById('editMode');
        editMode.classList.remove('hidden');
    });

    // Change Password logic
    listenIfExists('#newPassword', 'input', () => {
        const oldPassword = document.getElementById('oldPassword');
        const newPassword = document.getElementById('newPassword');

        if (newPassword.value.trim() !== '') {
            oldPassword.removeAttribute('disabled');   // bật
        } else {
            oldPassword.setAttribute('disabled', true); // tắt
            oldPassword.value = ''; // clear luôn cho chắc
        }
    });

    listenIfExists('#cancelEdit', 'click', () => {
        const editMode = document.getElementById('editMode');
        editMode.classList.add('hidden');
    });

    // Lưu lại chỉnh sửa
    listenIfExists('#saveEdit', 'click', async () => {
        const avatar = document.getElementById('avatar');
        const usernameDisplay = document.getElementById('usernameDisplay');
        const editName = document.getElementById('editName');
        const editEmail = document.getElementById('editEmail');
        const oldPassword = document.getElementById('oldPassword');
        const newPassword = document.getElementById('newPassword');
        const userInfo = Model.getLocalStorageJSON('userInfo') || {};

        // Kiểm tra xem có file nào đã được chọn không
        if (Model.selectedFile) {
            // Gọi hàm xử lý chính với file đã được lưu
            if (await Controller.ChangeAvatar(Model.selectedFile, avatar)) {
                userInfo.Avatar = avatar.src;
            }
        }

        // Username
        if (editName && editName.value.trim() !== '') {
            if (await Controller.ChangeUsername(editName.value.trim())) {
                userInfo.Username = editName?.value.trim() || userInfo.Username;
                usernameDisplay.textContent = editName.value.trim();
            }

        }

        // Password
        if (newPassword && newPassword.value.trim() !== '') {
            if (oldPassword && oldPassword.value.trim() !== '') {
                if (await Controller.ChangePassword(oldPassword.value.trim(), newPassword.value.trim())) {
                    userInfo.Password = newPassword?.value.trim() || userInfo.Password;
                }
            } else {
                check = false;
                View.showWarning("Vui lòng nhập mật khẩu cũ để đổi mật khẩu");
            }
        }

        // Email
        if (editEmail && editEmail.value.trim() !== '') {
            if (await Controller.ChangeEmail(editEmail.value.trim())) {
                userInfo.Email = editEmail?.value.trim() || userInfo.Email;
            }

        }

        Model.setLocalStorageJSON('userInfo', userInfo);

        editMode.classList.add('hidden');
    });

    listenIfExists('#closeSearchPopup', 'click', () => {
        searchPopup.classList.remove('active');
    });

    listenIfExists('#searchInput', 'blur', () => {
        setTimeout(() => {
            searchPopup.classList.remove('active');
        }, 200);
    });

    listenIfExists('#closeSearchPopup', 'keypress', (e) => {
        const searchPopup = document.getElementById('searchPopup');
        const searchInput = document.getElementById('searchInput');

        searchPopup.classList.remove('active');

        if (e.key === 'Enter' && activePosterIndex !== null) {

        }
    });

    // ========= Review =============
    listenIfExists('#clearFilters', 'click', () => {

    });
    listenIfExists('#loadMoreBtn', 'click', () => {

    });
    listenIfExists('#addReviewBtn', 'click', () => {

    });
    listenIfExists('#closeModal', 'click', () => {

    });
    listenIfExists('#cancelReview', 'click', () => {

    });
    listenIfExists('#addReviewModal', 'click', () => {

    });

    // ======== Commment =============
    listenIfExists('#game-review', 'click', (e) => {
        const btn = e.target.closest('button'); // tìm button gần nhất từ target
        if (!btn) return;

        // Nếu button nằm trong reaction area
        if (btn.closest('.comment-reactions')) {
            console.log('Clicked reaction button:', btn);

            // Ví dụ tăng count
            const countSpan = btn.querySelector('span');
            if (countSpan) {
                let count = parseInt(countSpan.textContent) || 0;
                countSpan.textContent = count + 1;
            }
        }
    });

    // ======== Reaction =============



    // ======== Follower ============



    // ======== Following ===========




    // ======== Home ===========
    listenIfExists('#pagination', 'click', (e) => {
        const btn = e.target.closest('button');
        if (!btn) return;

        const dataGame = Model.getLocalStorageJSON('gameData') || [];
        const itemsPerPage = 10;
        const numberPage = Math.ceil(dataGame.length / itemsPerPage); // Làm tròn lên để đảm bảo đủ trang

        // Nếu là nút số trang
        if (btn.classList.contains('page-btn')) {
            Model.currentPagination = parseInt(btn.dataset.page);
            Controller.GetGamePagination(dataGame, Model.currentPagination, itemsPerPage);
            View.updatePaginationUI(Model.currentPagination, numberPage);
        }

        // Nếu là nút prev
        if (btn.classList.contains('prev-btn')) {
            if (Model.currentPagination > 1) {
                Model.currentPagination -= 1; // Giảm trang hiện tại đi 1
                Controller.GetGamePagination(dataGame, Model.currentPagination, itemsPerPage);
                View.updatePaginationUI(Model.currentPagination, numberPage);
            }
        }

        // Nếu là nút next
        if (btn.classList.contains('next-btn')) {
            if (Model.currentPagination < numberPage) {
                Model.currentPagination += 1; // Tăng trang hiện tại lên 1
                Controller.GetGamePagination(dataGame, Model.currentPagination, itemsPerPage);
                View.updatePaginationUI(Model.currentPagination, numberPage);
            }
        }
    });


    // ========  Game-Detail ==========

    // Lưu rating hiện tại
    Model.reviewRating = 0;

    // Gắn sự kiện cho từng sao
    for (let i = 1; i <= 10; i++) {
        listenIfExists(`.star[data-value="${i}"]`, 'click', (e) => {
            const value = parseInt(e.target.getAttribute('data-value'));

            if (Model.reviewRating === value) {
                Model.reviewRating = 0;
            } else {
                Model.reviewRating = value;
            }
        });
    }

    listenIfExists('#Savebtn', 'click', () => {
        const rateLabel = document.getElementById('rateLabel');
        const contentReview = document.getElementById('contentReview')?.value || '';
        const gameId = View.getParamValue('gameId');

        if (rateLabel.textContent == "Rated" && contentReview.length > 0) {
            Controller.SaveReview(gameId, contentReview, Model.reviewRating)
        }
        else {
            View.showWarning("Bạn phải rate và viết content!");
        }

        document.getElementById('reviewModal').classList.add('hidden');
    })
    listenIfExists('#tabReviews', 'click', () => {
        const gameId = View.getParamValue('gameId');
        Controller.ShowReviews(gameId);
    })

    listenIfExists('#tabServices', 'click', () => {
        const gameId = View.getParamValue('gameId');
        Controller.ShowService(gameId);
    })

    listenIfExists('#tabGenres', 'click', () => {
        const gameId = View.getParamValue('gameId');
        Controller.ShowGenre(gameId);
    })

    listenIfExists('#tabDetails', 'click', () => {
        const gameId = View.getParamValue('gameId');
        Controller.ShowDetails(gameId);

    })


    // ======== Game-Review =========



    // ======== 


    // ========= Window event =========
    listenWindow('load', () => {
        View.showLoading();
        const page = View.getPageNow();
        Controller.ShowUserInfo();

        const token = Model.getAuthToken();
        if (token != null) {
            if (Model.isAuthTokenValid(token)) {
                if (page == Pages.Page.INDEX || page == Pages.Page.AUTH || page == Pages.Page.REGISTER)
                    View.goTo(Pages.Page.HOME);
            } else {
                Model.deleteAuthToken();
                if (page != Pages.Page.INDEX && page != Pages.Page.AUTH && page != Pages.Page.REGISTER)
                    View.goTo(Pages.Page.AUTH);
            }
        }

        const curPage = View.getPageNow();
        if (curPage == Pages.Page.HOME) {
            Controller.LoadHomeContent(Model.currentPagination, 10);
        }

        if (curPage == Pages.Page.GAME_DETAIL) {
            const gameId = View.getParamValue('gameId')
            Controller.ShowGameDetail(gameId);
            Controller.ShowReviews(gameId);
        }

        if (curPage == Pages.Page.GAME_REVIEW) {
            const reviewId = View.getParamValue('reviewId');
            Controller.ShowReviewDetail(reviewId);
        }


        setTimeout(() => View.hideLoading(), 250);
    });

    window.addEventListener('pageshow', (event) => {
        if (event.persisted) {
            window.location.reload();
        }
    });

    listenWindow('offline', () => {
        View.showWarning('Bạn đang ngoại tuyến');
        console.log('Offline event fired');
    });

    listenWindow('online', () => {
        View.showSuccess('Bạn đã kết nối lại internet');
        console.log('Online event fired');
    });

    ['mousemove', 'keydown', 'scroll', 'click'].forEach(event => {
        listenWindow(event, () => {
            Controller.resetIdleTimer()
        });
    });

    Controller.resetIdleTimer();
});

document.addEventListener('DOMContentLoaded', () => {
    // Load More Games button
    listenIfExists('#loadMoreBtn', 'click', () => {
        const dataGame = Model.getLocalStorageJSON('gameData') || [];
        const itemsPerPage = 6; // Match the grid columns (6 items per page)
        Model.currentPagination = Model.currentPagination || 1;
        Model.currentPagination += 1;
        const startIndex = (Model.currentPagination - 1) * itemsPerPage;
        const endIndex = Model.currentPagination * itemsPerPage;
        const newGames = dataGame.slice(startIndex, endIndex);
        if (newGames.length > 0) {
            View.showGameListPagination(newGames);
        } else {
            document.getElementById('loadMoreBtn').disabled = true;
        }
    });
});

listenWindow('load', () => {
    View.showLoading();
    const page = View.getPageNow();
    Controller.ShowUserInfo();

    const token = Model.getAuthToken();
    if (token != null) {
        if (Model.isAuthTokenValid(token)) {
            if (page == Pages.Page.INDEX || page == Pages.Page.AUTH || page == Pages.Page.REGISTER)
                View.goTo(Pages.Page.HOME);
        } else {
            Model.deleteAuthToken();
            if (page != Pages.Page.INDEX && page != Pages.Page.AUTH && page != Pages.Page.REGISTER)
                View.goTo(Pages.Page.AUTH);
        }
    }

    const curPage = View.getPageNow();
    if (curPage == Pages.Page.GAMES) {
        Controller.LoadGamesContent(Model.currentPagination || 1, 6); // Initial load with 6 items
    } else if (curPage == Pages.Page.HOME) {
        Controller.LoadHomeContent(Model.currentPagination || 1, 10);
    } else if (curPage == Pages.Page.GAME_DETAIL) {
        const gameId = View.getParamValue('gameId');
        Controller.ShowGameDetail(gameId);
        Controller.ShowReviews(gameId);
    }

    setTimeout(() => View.hideLoading(), 250);
});

// Game card click handler (event delegation for dynamic cards)
document.addEventListener('click', (e) => {
    const card = e.target.closest('.group');
    if (card && card.dataset.gameId) {
        const gameId = card.dataset.gameId;
        window.location.href = `game-detail.html?gameId=${gameId}`;
    }
});


// Replace the existing #Savebtn listener
listenIfExists('#Savebtn', 'click', async () => {
    const rateLabel = document.getElementById('rateLabel');
    const contentReview = document.getElementById('contentReview')?.value || '';
    const gameId = View.getParamValue('gameId');

    if (rateLabel.textContent === "Rated" && contentReview.length > 0) {
        const success = await Controller.SaveReview(gameId, contentReview, Model.reviewRating);
        if (success) {
            document.getElementById('reviewModal').classList.add('hidden');
            if (View.getPageNow() === Pages.Page.PROFILE) {
                await Controller.ShowUserInfo();
            }
        }
    } else {
        View.showWarning("Bạn phải rate và viết content!");
    }
});

// Replace the existing load listener (keep only one)
listenWindow('load', () => {
    View.showLoading();
    const page = View.getPageNow();
    Controller.ShowUserInfo();

    const token = Model.getAuthToken();
    if (token != null) {
        if (Model.isAuthTokenValid(token)) {
            if (page == Pages.Page.INDEX || page == Pages.Page.AUTH || page == Pages.Page.REGISTER)
                View.goTo(Pages.Page.HOME);
        } else {
            Model.deleteAuthToken();
            if (page != Pages.Page.INDEX && page != Pages.Page.AUTH && page != Pages.Page.REGISTER)
                View.goTo(Pages.Page.AUTH);
        }
    }

    const curPage = View.getPageNow();
    if (curPage == Pages.Page.HOME) {
        Controller.LoadHomeContent(Model.currentPagination || 1, 10);
    } else if (curPage == Pages.Page.PROFILE) {
        Controller.ShowUserInfo(); // Ensure it runs for profile
    } else if (curPage == Pages.Page.GAME_DETAIL) {
        const gameId = View.getParamValue('gameId');
        Controller.ShowGameDetail(gameId);
        Controller.ShowReviews(gameId);
    }

    setTimeout(() => View.hideLoading(), 250);
});