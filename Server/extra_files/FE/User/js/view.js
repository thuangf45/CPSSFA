import { Model } from './model.js';

export class View {

    // ========= User ==========
    static showUserInfo(userInfo) {
        const avatar = document.getElementById('avatar');
        const usernameDisplay = document.getElementById('usernameDisplay');
        const numberFollower = document.getElementById('numberFollower');
        const numberFollowing = document.getElementById('numberFollowing');

        if (avatar) avatar.src = userInfo.Avatar || "https://via.placeholder.com/50x50";
        if (usernameDisplay) usernameDisplay.textContent = userInfo.Username || "Unknown";
        if (numberFollower) numberFollower.textContent = userInfo.NumberFollower || 0;
        if (numberFollowing) numberFollowing.textContent = userInfo.NumberFollowing || 0;
    }


    //========= Game Review =====
    
    // Get review
    static getCommentReview(comments) {
        const container = document.getElementById(game-review);
        if (!container) return;

        // Xóa cũ (nếu muốn load lại)
        container.innerHTML = "";

        comments.forEach(comment => {
            const commentHTML = `
             <div class="space-y-4" id ="game-review">
                <div class="flex items-start space-x-4 mb-4">
                    <img src="${comment.Avatar}" 
                         alt="${comment.Username}" 
                         class="w-10 h-10 rounded-full">
                    <div class="flex-1">
                        <div class="bg-gray-700 rounded-lg p-4">
                            <div class="flex items-center space-x-2 mb-2">
                                <h4 class="font-semibold text-sm">${comment.Username}</h4>
                                <span class="text-gray-400 text-xs">${comment.DateCreated}</span>
                            </div>
                            <p class="text-gray-300 text-sm">
                                ${comment.content}
                            </p>
                        </div>
                        <div class="flex items-center space-x-4 mt-2">
                            <button onclick="event.preventDefault(); openReactionPopup('like', ${comment.NumberReaction})" 
                                    class="flex items-center space-x-1 text-gray-400 hover:text-blue-400 text-sm">
                                <i class="fas fa-thumbs-up"></i>
                                <span>${comment.NumberReaction}</span>
                            </button>
                            
                        </div>
                    </div>
                </div>
            </div>
            `;
            container.insertAdjacentHTML("beforeend", commentHTML);
        });
    }
    //reaction pop up
    static viewReactions(type, count) {
        // Nếu popup đã có thì xóa trước
        const oldPopup = document.getElementById('reactionPopup');
        if (oldPopup) oldPopup.remove();

        // Tạo khung popup
        const reactionList = document.createElement('div');
        reactionList.id = 'reactionPopup';
        reactionList.className = 'fixed inset-0 bg-black bg-opacity-80 flex items-center justify-center z-50';
        reactionList.innerHTML = `
            <div class="bg-gray-900 text-white rounded-lg shadow-xl w-full max-w-md p-6 relative space-y-6">
                <div class="flex justify-between items-center text-blue-400 font-semibold text-lg">
                    <h2 id="reactionTitle" class="text-xl font-bold">
                        ${type.charAt(0).toUpperCase() + type.slice(1)} Reactions (${count})
                    </h2>
                    <button onclick="ReactionUI.close()" 
                            class="px-3 py-1 text-sm bg-gray-700 rounded hover:bg-gray-600">
                        Close
                    </button>
                </div>
                <div class="space-y-4 max-h-80 overflow-y-auto" id="reactionList"></div>
            </div>
        `;
        document.body.appendChild(reactionList);

        // Fake data (tạm để test)
        const reactions = {
            like: [
                { name: 'John Doe', avatar: 'https://via.placeholder.com/40x40' },
                { name: 'Sarah Wilson', avatar: 'https://via.placeholder.com/40x40' }
            ],
            heart: [
                { name: 'Mike Chen', avatar: 'https://via.placeholder.com/40x40' },
                { name: 'Emma Davis', avatar: 'https://via.placeholder.com/40x40' }
            ],
            sad: [
                { name: 'Tom Brown', avatar: 'https://via.placeholder.com/40x40' }
            ],
            laugh: [
                { name: 'Anna Lee', avatar: 'https://via.placeholder.com/40x40' },
                { name: 'Peter Smith', avatar: 'https://via.placeholder.com/40x40' }
            ]
        };

        const people = reactions[type] || [];
        const list = document.getElementById('reactionList');

        people.forEach(person => {
            const div = document.createElement('div');
            div.className = 'flex items-center space-x-4';
            div.innerHTML = `
                <img src="${person.avatar}" alt="${person.name}" class="w-10 h-10 rounded-full">
                <p class="font-semibold">${person.name}</p>
            `;
            list.appendChild(div);
        });
    }

    //get reaction to comment 
    async renderCommentReactions(commentId, reactions) {
        const container = document.querySelector(`.comment-reactions[data-comment="${commentId}"]`);
        if (!container) return;

        container.innerHTML = `
        <button data-reaction="like" class="comment-reaction-btn flex items-center space-x-1 text-gray-400 hover:text-blue-400 text-sm">
            <i class="fas fa-thumbs-up"></i>
            <span class="reaction-count">${reactions.like || 0}</span>
        </button>
        <button data-reaction="heart" class="comment-reaction-btn flex items-center space-x-1 text-gray-400 hover:text-red-400 text-sm">
            <i class="fas fa-heart"></i>
            <span class="reaction-count">${reactions.heart || 0}</span>
        </button>
        <button data-reaction="sad" class="comment-reaction-btn flex items-center space-x-1 text-gray-400 hover:text-yellow-400 text-sm">
            <i class="far fa-frown"></i>
            <span class="reaction-count">${reactions.sad || 0}</span>
        </button>
        <button data-reaction="laugh" class="comment-reaction-btn flex items-center space-x-1 text-gray-400 hover:text-yellow-400 text-sm">
            <i class="far fa-laugh"></i>
            <span class="reaction-count">${reactions.laugh || 0}</span>
        </button>
    `;
    }


    //get reaction to review 
    async renderReviewReactions(reviewId, reactions) {
        const container = document.getElementById("main-reactions");
        if (!container) return;

        container.innerHTML = `
        <button data-reaction="like" class="reaction-btn flex items-center space-x-2 text-gray-400 hover:text-blue-400 transition-colors">
            <i class="fas fa-thumbs-up text-lg"></i>
            <span class="font-semibold reaction-count">${reactions.like || 0}</span>
        </button>
        <button data-reaction="heart" class="reaction-btn flex items-center space-x-2 text-gray-400 hover:text-red-400 transition-colors">
            <i class="fas fa-heart text-lg"></i>
            <span class="font-semibold reaction-count">${reactions.heart || 0}</span>
        </button>
        <button data-reaction="sad" class="reaction-btn flex items-center space-x-2 text-gray-400 hover:text-yellow-400 transition-colors">
            <i class="far fa-frown text-lg"></i>
            <span class="font-semibold reaction-count">${reactions.sad || 0}</span>
        </button>
        <button data-reaction="laugh" class="reaction-btn flex items-center space-x-2 text-gray-400 hover:text-yellow-400 transition-colors">
            <i class="far fa-laugh text-lg"></i>
            <span class="font-semibold reaction-count">${reactions.laugh || 0}</span>
        </button>
    `;
    }

    static renderReviewCard(dataReview, dataComment, elementId = "reviews") {
        const container = document.getElementById(elementId);
        if (!container) return;

        container.innerHTML = '';
            
        const body = document.querySelector("body");
        if (dataReview.Backdrop) {
            body.style.backgroundImage = `url(${dataReview .Backdrop})`;
            body.style.backgroundSize = "cover";
            body.style.backgroundPosition = "center";
            body.style.backgroundRepeat = "no-repeat";

            // Thêm lớp phủ mờ (overlay) nếu muốn chữ dễ đọc
            body.style.backgroundColor = "rgba(0,0,0,0.7)";
            body.style.backgroundBlendMode = "overlay";
        } else {
            // fallback: màu mặc định
            body.style.background = "#111827"; // bg-gray-900
        }



        // ⭐ Tạo rating stars
        const stars = Array.from({ length: 5 }, (_, i) => {
            return `<i class="fas fa-star ${i < dataReview.Rating ? "text-green-400" : "text-gray-600"}"></i>`;
        }).join("");

        // 🟢 Tạo review card
        let html = `
        <div class="bg-gray-800 rounded-lg p-8 shadow-xl mb-8">
            <div class="flex flex-col lg:flex-row lg:space-x-8">
                <!-- Left Side - Game Poster -->
                <div class="flex-shrink-0 mb-6 lg:mb-0">
                    <a href="game-detail.html" class="block hover:opacity-80 transition-opacity">
                        <img src="${dataReview.Poster}" alt="${dataReview.Title}" 
                            class="w-48 h-72 rounded-lg shadow-lg mx-auto lg:mx-0">
                    </a>
                </div>

                <!-- Right Side - Review Content -->
                <div class="flex-1">
                    <!-- Header -->
                    <div class="mb-6">
                        <div class="flex items-center space-x-4 mb-4">
                            <img src="${dataReview.Avatar}" alt="${dataReview.Username}" 
                                class="w-12 h-12 rounded-full">
                            <div>
                                <h2 class="font-semibold text-lg">${dataReview.Username}</h2>
                                <p class="text-gray-400 text-sm">${dataReview.DateCreated}</p>
                            </div>
                        </div>

                        <div class="mb-4">
                            <h1 class="text-3xl font-bold mb-2">${dataReview.Title}</h1>
                            <div class="flex items-center space-x-4">
                                <div class="flex text-xl">${stars}</div>
                            </div>
                        </div>
                    </div>

                    <!-- Review Text -->
                    <div class="mb-6">
                        <div class="prose prose-lg text-gray-300 max-w-none">
                            ${dataReview.Content}
                        </div>
                    </div>

                    <!-- Actions -->
                    <div class="flex items-center justify-between pt-6 border-t border-gray-700">
                        <div class="flex items-center space-x-4" id="main-reactions">
                            <button onclick="GameUI.addReaction(${dataReview.Id}, 'like')" 
                                class="flex items-center space-x-2 text-gray-400 hover:text-blue-400 transition-colors">
                                <i class="fas fa-thumbs-up text-lg"></i>
                                <span>${dataReview.NumberReaction || 0}</span>
                            </button>
                        </div>

                        <button class="text-gray-400 hover:text-blue-400 transition-colors">
                            <i class="fas fa-copy text-lg"></i>
                        </button>
                    </div>
                </div>
            </div>
        </div>
        `;

    
        // 🟡 Comments Section
        let commentsHtml = `
        <div class="mt-8 bg-gray-800 rounded-lg p-6">
            <h3 class="text-xl font-bold mb-6">Comments ${dataReview.NumberComment}</h3>
            <!-- Comment Form -->
            <div class="mb-6">
                <div class="flex items-start space-x-4">
                    <img src="${Model.getLocalStorageJSON('userInfo').Avatar}" alt="Loading..." id="avatar" class="w-10 h-10 rounded-full">
                    <div class="flex-1">
                        <textarea id="commentContent" placeholder="Write a comment..."
                            class="w-full bg-gray-700 text-white rounded-lg p-3 resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
                            rows="3"></textarea>
                        <div class="flex justify-end mt-2">
                            <button id="commentBtn" class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg text-sm font-medium">
                                Post Comment
                            </button>
                        </div>
                    </div>
                </div>
            </div>
            <!-- Comments List -->
            <div class="space-y-4" id="game-review">
        `;

        // 🔵 Render từng comment trong dataComment
        (dataComment || []).forEach(cmt => {
            commentsHtml += `
            <div class="flex items-start space-x-4">
                <img src="${cmt.Avatar}" alt="${cmt.Username}" class="w-10 h-10 rounded-full">
                <div class="flex-1">
                    <div class="bg-gray-700 rounded-lg p-4">
                        <div class="flex items-center space-x-2 mb-2">
                            <h4 class="font-semibold text-sm">${cmt.Username}</h4>
                            <span class="text-gray-400 text-xs">${cmt.DateCreated}</span>
                        </div>
                        <p class="text-gray-300 text-sm">${cmt.Content}</p>
                    </div>
                    <div class="flex items-center space-x-4 mt-2 comment-reactions">
                        <button class="flex items-center space-x-1 text-gray-400 hover:text-blue-400 text-sm">
                            <i class="fas fa-thumbs-up"></i>
                            <span>${cmt.NumberReaction || 0}</span>
                        </button>
                    </div>
                </div>
            </div>`;
        });

        commentsHtml += `</div></div>`;

        // Gộp review + comments
        container.innerHTML = html + commentsHtml;
    }





    // Load tất cả comments trong trang
    static async loadAllCommentReactions() {
        const commentDivs = document.querySelectorAll(".comment-reactions");
        for (const div of commentDivs) {
            const commentId = div.dataset.comment; // ví dụ: "comment1"
            await ReactionHandler.loadCommentReactions(commentId);
        }
    }
    static close() {
        const popup = document.getElementById('reactionPopup');
        if (popup) popup.remove();
    }

    // ======== Review ==========
    //ShowReview
    static showReview(containerId, review) {
        const stars = '★'.repeat(review.rating) + '☆'.repeat(5 - review.rating);
        const formattedDate = new Date(review.date).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });

        const reviewHTML = `
            <div class="bg-gray-800 rounded-lg p-6 review-card fade-in" data-review-id="${review.id}">
                <div class="flex gap-6">
                    <!-- Game Poster -->
                    <div class="flex-shrink-0">
                        <img src="${review.poster}" alt="${review.title}" 
                             class="w-20 h-28 object-cover rounded-lg shadow-lg cursor-pointer hover:opacity-80 transition-opacity game-poster">
                    </div>
                    
                    <!-- Review Content -->
                    <div class="flex-1">
                        <div class="flex items-center justify-between mb-3">
                            <div>
                                <h3 class="text-xl font-bold text-white mb-1 cursor-pointer hover:text-blue-400 transition-colors game-title">${review.title}</h3>
                                <div class="flex items-center gap-2">
                                    <div class="text-green-400 text-lg">
                                        ${stars}
                                    </div>
                                </div>
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
                        
                        <!-- Tags -->
                        <div class="flex flex-wrap gap-2 mt-4">
                            ${review.tags.map(tag => `<span class="bg-blue-600 text-xs px-2 py-1 rounded-full cursor-pointer hover:bg-blue-700 transition-colors" onclick="searchByTag('${tag}')">#${tag}</span>`).join('')}
                        </div>

                        <!-- Action buttons -->
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

        // Chèn vào container
        const container = document.getElementById(containerId);
        if (container) {
            container.insertAdjacentHTML('beforeend', reviewHTML);
        }
    }





    //=============== Game-detail=======================

    // Cập nhật modal review
    static populateReviewModal(gameData) {
        const gameId = this.getGameIdFromUrl();
        if (!gameId) return;

        // Tìm game theo id
        const game = gameData.find(g => String(g.GameId) === String(gameId));
        if (!game) return;

        // Lấy modal elements
        const modal = document.getElementById("reviewModal");
        const titleEl = modal.querySelector("h2");
        const posterEl = modal.querySelector("img");
        const dateEl = modal.querySelector("#reviewDate");

        // Cập nhật thông tin
        if (titleEl) {
            titleEl.innerHTML = `${game.Title} <span class="text-gray-400 text-lg">${game.Year}</span>`;
        }
        if (posterEl) {
            posterEl.src = game.Poster || "https://via.placeholder.com/100x150";
        }
        if (dateEl) {
            dateEl.textContent = new Date().toLocaleDateString();
        }
    }


    // reviews tab
    static showGameDetail(game, elementId = "gameDetail") {
        const container = document.getElementById(elementId);
        if (!container) return;

        container.innerHTML = `
        <!-- Backdrop Image -->
        <div class="relative h-96 bg-cover bg-center"
            style="background-image: url('${game.Backdrop}')">
            <div class="backdrop-overlay absolute inset-0"></div>

            <!-- Game Card -->
            <div class="absolute bottom-6 left-6 flex items-end space-x-6 z-10 game-card-container">
                <!-- Poster -->
                <div class="flex-shrink-0">
                    <img src="${game.Poster}"
                        alt="${game.Title}" class="w-48 h-72 rounded-lg shadow-2xl">
                </div>

                <!-- Game Info -->
                <div class="pb-4 game-info">
                    <h1 class="text-4xl font-bold mb-2">${game.Title}</h1>
                    <div class="flex items-center space-x-6 mb-4 rating-container">
                        <!-- Rating -->
                        <div class="flex items-center space-x-3">
                            <span class="text-3xl font-bold text-green-400">${game.AvgRating}/10</span>
                        </div>

                        <!-- Action Button -->
                        <div class="relative action-container">
                            <button id="actionBtn" onclick="openReviewModal()"
                                class="bg-blue-600 hover:bg-blue-700 px-6 py-3 rounded-lg font-semibold transition-colors">
                                Write Review
                            </button>
                        </div>
                    </div>
                </div>
            </div>  
        </div>`;
    }



    // reviews = [ { UserName, Avatar, Rating, Date, Content, Reactions } ]
    static renderReview(r, containerId = "review-list") {
        const container = document.getElementById(containerId);
        if (!container) return;

        container.innerHTML = '';

        for (var i = 0; i < r.length; i++) {
            const reviewEl = document.createElement("div");
            reviewEl.className = " bg-gray-800 rounded-lg p-6 review-content";
            reviewEl.dataset.id = r[i].ReviewId;

            reviewEl.innerHTML = `
        <a href="game-review.html?reviewId=${r[i].ReviewId}" class="flex items-start space-x-4">
            <img  src="${r[i].Avatar}" alt="${r[i].Username}" class="w-12 h-12 rounded-full">
            <div class="flex-1">
                <div class="flex items-center space-x-3 mb-2">
                    <a class="font-semibold hover:underline">
                        ${r[i].Username}
                    </a>
                    <span class="text-green-400 text-2xl">
                        ${"★".repeat(r[i].Rating)}${"☆".repeat(10 - r[i].Rating)}
                    </span>
                    <span class="text-sm text-gray-400">${r[i].DateCreated}</span>
                </div>
                <p class="text-gray-300 mb-3">${r[i].Content}</p>
                <div class="flex items-center space-x-4">
                    <button onclick="GameUI.addReaction(${r[i].Id}, 'like')" 
                        class="flex items-center space-x-2 text-gray-400 hover:text-blue-400">
                        <i class="fas fa-thumbs-up"></i>
                        <span onclick="event.stopPropagation(); GameUI.showReactionPopup(${r[i].Id}, 'like')">
                            ${r[i].NumberReaction || 0} 
                        </span>
                    </button>
                </div>
            </div>
        </a>
        `;
            container.appendChild(reviewEl);
        }
    }



    // này chắc để trong event mới đúng nhưng mà thôi kệ 
    // 2. Khi bấm icon → tăng số
    static addReaction(reviewId, type) {
        const reviewEl = document.querySelector(`[data-id="${reviewId}"]`);
        if (!reviewEl) return;

        const span = reviewEl.querySelector(`button[onclick*="'${type}'"] span`);
        let count = parseInt(span.textContent) || 0;
        span.textContent = count + 1;

        // TODO: gọi API lưu reaction
        console.log(`Added ${type} to review ${reviewId}`);
    }

    // 3. Khi bấm vào số → hiện popup danh sách người react
    static showReactionPopup(reviewId, type) {
        // giả lập: gọi API để lấy danh sách người dùng
        const users = ["Alice", "Bob", "Charlie", "David"];

        const popup = document.createElement("div");
        popup.className = "fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-50";
        popup.innerHTML = `
            <div class="bg-gray-900 text-white rounded-xl p-6 w-80">
                <h3 class="text-lg font-semibold mb-4">People who reacted with ${type}</h3>
                <ul class="space-y-2 max-h-60 overflow-y-auto">
                    ${users.map(u => `<li class="p-2 bg-gray-800 rounded">${u}</li>`).join("")}
                </ul>
                <button onclick="this.closest('.fixed').remove()" 
                    class="mt-4 bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded">Close</button>
            </div>
        `;
        document.body.appendChild(popup);
    }



    // detail tab
    static renderDetailsTab(description, details, elementId = "details") {
        const container = document.getElementById(elementId);
        if (!container) return;

        container.innerHTML = `
            <h2 class="text-2xl font-bold mb-6">Details</h2>
            <div class="bg-gray-800 rounded-lg p-6">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div class="md:col-span-2">
                        <h3 class="font-semibold text-gray-400 mb-2">Description</h3>
                        <p class="text-white">${description}</p>
                    </div>
                    <div class="md:col-span-2">
                        <h3 class="font-semibold text-gray-400 mb-2">Details</h3>
                        <p class="text-white">${details}</p>
                    </div>
                </div>
            </div>
        `;
    }

    // genre tab 
    static renderGenresTab(genres, elementId = "genres") {
        const container = document.getElementById(elementId);
        if (!container) return;

        // Nếu genres là string -> tách ra thành mảng
        let genreList = Array.isArray(genres)
            ? genres
            : genres.split(",").map(g => g.trim()).filter(g => g.length > 0);

        const genreSpans = genreList.map(
            g => `<span class="bg-blue-600 text-white px-4 py-2 rounded-full text-sm font-medium">${g}</span>`
        ).join("");

        container.innerHTML = `
            <h2 class="text-2xl font-bold mb-6">Genres</h2>
            <div class="bg-gray-800 rounded-lg p-6">
                <div class="flex flex-wrap gap-3">
                    ${genreSpans}
                </div>
            </div>
        `;
    }
    // service 

    static renderServicesTab(services, elementId = "services") {
        const container = document.getElementById(elementId);
        if (!container) return;

        // Nếu services là string -> tách ra thành mảng
        let serviceList = Array.isArray(services)
            ? services
            : services.split(",").map(s => s.trim()).filter(s => s.length > 0);

        const serviceDivs = serviceList.map(s => `
        <div class="p-3 bg-gray-700 rounded-lg text-center font-medium">
            ${s}
        </div>
    `).join("");

        container.innerHTML = `
        <h2 class="text-2xl font-bold mb-6">Available On</h2>
        <div class="bg-gray-800 rounded-lg p-6">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                ${serviceDivs}
            </div>
        </div>
    `;
    }



    //====================== Show Game in Home ================================

    // GameId, Title, Poster
    static showGame(gameData, elementId = 'gamePagination') {
        const grid = document.getElementById(elementId);
        if (!grid) return;

        // Xóa nội dung cũ (tránh trùng lặp khi load lại)
        grid.innerHTML = "";

        for (let i = 0; i < gameData.length; i++) {
            // Tạo div làm game item
            const gameItem = document.createElement('div');
            gameItem.className = "game-item";

            // Thêm nội dung
            gameItem.innerHTML = `
                <a href="game-detail.html?gameId=${gameData[i].GameId}" class="block">
                    <img src="${gameData[i].Poster}" 
                        alt="Game Poster" 
                        class="game-poster rounded-lg">
                    <h3 class="text-sm font-medium mt-2 text-center">${gameData[i].Title}</h3>
                </a>
            `;

            // Append vào grid
            grid.appendChild(gameItem);
        }
    }
    static showGamePopular(gameData) {
        View.showGame(gameData, 'gamePopular');
    }

    static showGameBest(gameData) {
        View.showGame(gameData, 'gameBest');
    }

    static showGamePagination(gameData) {
        View.showGame(gameData, 'gamePagination');
    }


    static updatePaginationUI(currentPage, totalPages) {
        const paginationContainer = document.getElementById("pagination");
        paginationContainer.innerHTML = ""; // xoá các nút cũ

        const maxVisible = 3; // số nút hiển thị cùng lúc

        // Tính toán phạm vi hiển thị
        let start = Math.max(1, currentPage - Math.floor(maxVisible / 2));
        let end = start + maxVisible - 1;

        if (end > totalPages) {
            end = totalPages;
            start = Math.max(1, end - maxVisible + 1);
        }

        // Nút Previous
        if (currentPage > 1) {
            const prevBtn = document.createElement("button");
            prevBtn.textContent = "«";
            prevBtn.className = "prev-btn bg-blue-600 hover:bg-blue-700 text-white px-3 py-2 rounded-lg transition-colors duration-200 font-medium";
            prevBtn.dataset.page = currentPage - 1;
            paginationContainer.appendChild(prevBtn);
        }

        // Nút số trang
        for (let i = start; i <= end; i++) {
            const btn = document.createElement("button");
            btn.textContent = i;
            btn.dataset.page = i;
            btn.className = "page-btn bg-blue-600 hover:bg-blue-700 text-white px-3 py-2 rounded-lg transition-colors duration-200 font-medium";
            if (i === currentPage) {
                btn.classList.add("bg-blue-800"); // active
            }
            paginationContainer.appendChild(btn);
        }

        // Nút Next
        if (currentPage < totalPages) {
            const nextBtn = document.createElement("button");
            nextBtn.textContent = "»";
            nextBtn.className = "next-btn bg-blue-600 hover:bg-blue-700 text-white px-3 py-2 rounded-lg transition-colors duration-200 font-medium";
            nextBtn.dataset.page = currentPage + 1;
            paginationContainer.appendChild(nextBtn);
        }
    }











    // ==========  View base ================
    constructor() {
        View.init();
    }

    static init() {
        View.createNotificationContainer();
    }

    static goTo(page) {
        // Điều hướng thật
        window.location.href = page;
    }
    static getPageNow() {
        const path = window.location.pathname;
        if (path === '/' || path === '') {
            return 'index.html'; // chuẩn hóa thành index.html
        }

        return path.substring(path.lastIndexOf('/') + 1);
    }

    static getParamValue(param) {
        // Lấy query string từ URL
        const params = new URLSearchParams(window.location.search);
        // Lấy giá trị của id
        const value = params.get(param);
        return value;

    }
    // Existing notification methods...
    static createNotificationContainer() {
        if (!document.getElementById('notificationContainer')) {
            const container = document.createElement('div');
            container.id = 'notificationContainer';
            container.className = 'fixed top-4 right-4 z-50 space-y-2';
            document.body.appendChild(container);
        }
    }

    static showLoading(message = 'Processing...') {
        // Check if loading overlay exists, if not create it
        let overlay = document.getElementById('loadingOverlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'loadingOverlay';
            overlay.className = 'fixed inset-0 bg-black/50 flex items-center justify-center z-50 hidden';
            overlay.innerHTML = `
                <div class="bg-gray-800 rounded-lg p-6 flex items-center space-x-4">
                    <div class="animate-spin rounded-full h-8 w-8 border-2 border-white border-t-transparent"></div>
                    <p class="text-white">${message}</p>
                </div>
            `;
            document.body.appendChild(overlay);
        }

        const messageElement = overlay.querySelector('p');
        if (messageElement) {
            messageElement.textContent = message;
        }
        overlay.classList.remove('hidden');
    }

    static hideLoading() {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) {
            overlay.classList.add('hidden');
        }
    }

    static showError(message, duration = 5000) {
        View.showNotification(message, 'error', duration);
    }

    static showSuccess(message, duration = 3000) {
        View.showNotification(message, 'success', duration);
    }

    static showInfo(message, duration = 4000) {
        View.showNotification(message, 'info', duration);
    }

    static showWarning(message, duration = 4000) {
        View.showNotification(message, 'warning', duration);
    }

    static showNotification(message, type = 'info', duration = 4000) {
        const container = document.getElementById('notificationContainer');
        if (!container) return;

        const notification = document.createElement('div');
        notification.className = `notification bg-gray-800 p-4 rounded-lg shadow-lg transform transition-all duration-300 translate-x-full opacity-0 max-w-sm border-l-4 ${View.getNotificationClasses(type)}`;

        const iconHtml = View.getNotificationIcon(type);

        notification.innerHTML = `
            <div class="flex items-start gap-3">
                <div class="flex-shrink-0">
                    ${iconHtml}
                </div>
                <div class="flex-1">
                    <p class="text-white text-sm font-medium">${message}</p>
                </div>
                <button class="flex-shrink-0 text-white/60 hover:text-white/80 ml-2" onclick="this.parentElement.parentElement.remove()">
                    <i class="fas fa-times text-xs"></i>
                </button>
            </div>
        `;

        container.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.classList.remove('translate-x-full', 'opacity-0');
        }, 100);

        // Auto remove
        if (duration > 0) {
            setTimeout(() => {
                View.removeNotification(notification);
            }, duration);
        }
    }

    static removeNotification(notification) {
        notification.classList.add('translate-x-full', 'opacity-0');
        setTimeout(() => {
            if (notification.parentElement) {
                notification.parentElement.removeChild(notification);
            }
        }, 300);
    }

    static getNotificationClasses(type) {
        switch (type) {
            case 'success':
                return 'border-green-500 bg-green-900/20';
            case 'error':
                return 'border-red-500 bg-red-900/20';
            case 'warning':
                return 'border-yellow-500 bg-yellow-900/20';
            case 'info':
            default:
                return 'border-blue-500 bg-blue-900/20';
        }
    }

    static getNotificationIcon(type) {
        switch (type) {
            case 'success':
                return '<i class="fas fa-check-circle text-green-400"></i>';
            case 'error':
                return '<i class="fas fa-times-circle text-red-400"></i>';
            case 'warning':
                return '<i class="fas fa-exclamation-triangle text-yellow-400"></i>';
            case 'info':
            default:
                return '<i class="fas fa-info-circle text-blue-400"></i>';
        }
    }
    // Lists Page Specific Methods
    displayGameSearchResults(results, addGameCallback) {
        const container = document.getElementById('gameSearchResults');
        if (!container) return;

        if (results.length === 0) {
            container.innerHTML = '<div class="text-gray-400 text-sm p-3">No games found</div>';
            return;
        }

        container.innerHTML = results.map(game => `
            <div class="game-search-item bg-gray-700 p-3 rounded cursor-pointer flex items-center justify-between hover:bg-gray-600 transition-colors" 
                 data-click="addGameToList" data-game-id="${game.id}">
                <div>
                    <div class="text-white font-medium">${game.title}</div>
                    <div class="text-gray-400 text-sm">${game.genre} • ${game.year}</div>
                </div>
                <i class="fas fa-plus text-blue-400"></i>
            </div>
        `).join('');
    }

    clearGameSearchResults() {
        const container = document.getElementById('gameSearchResults');
        if (container) {
            container.innerHTML = '';
        }
    }

    clearGameSearch() {
        const searchInput = document.getElementById('gameSearch');
        if (searchInput) {
            searchInput.value = '';
        }
        View.clearGameSearchResults();
    }

    // Input validation methods
    updateInputValidation(input, isValid, errorMessage = '') {
        const fieldContainer = input.parentElement;
        let errorElement = fieldContainer.querySelector('.field-error');

        // Remove existing error styling
        input.classList.remove('border-red-500', 'border-2', 'border-green-500');

        if (errorElement) {
            errorElement.remove();
        }

        if (!isValid && errorMessage) {
            // Add error styling
            input.classList.add('border-red-500', 'border-2');

            // Create error message
            errorElement = document.createElement('div');
            errorElement.className = 'field-error text-red-400 text-xs mt-1';
            errorElement.textContent = errorMessage;
            fieldContainer.appendChild(errorElement);
        } else if (isValid && input.value.trim()) {
            // Add success styling
            input.classList.add('border-green-500');
        }
    }

    updatePasswordStrength(passwordInput, strength) {
        let strengthIndicator = passwordInput.parentElement.querySelector('.password-strength');

        if (!strengthIndicator) {
            strengthIndicator = document.createElement('div');
            strengthIndicator.className = 'password-strength mt-2';
            passwordInput.parentElement.appendChild(strengthIndicator);
        }

        const strengthLevels = ['Very Weak', 'Weak', 'Fair', 'Good', 'Strong'];
        const strengthColors = ['bg-red-500', 'bg-orange-500', 'bg-yellow-500', 'bg-blue-500', 'bg-green-500'];

        const level = Math.min(strength, 4);
        const strengthText = strengthLevels[level] || 'Very Weak';
        const strengthColor = strengthColors[level] || 'bg-red-500';

        strengthIndicator.innerHTML = `
            <div class="flex items-center space-x-2">
                <div class="flex space-x-1">
                    ${Array.from({ length: 5 }, (_, i) => `
                        <div class="w-4 h-1 rounded ${i <= level ? strengthColor : 'bg-gray-600'}"></div>
                    `).join('')}
                </div>
                <span class="text-xs text-gray-400">${strengthText}</span>
            </div>
        `;
    }

    togglePassword(inputId, button) {
        const input = document.getElementById(inputId);
        if (!input) return;

        const icon = button.querySelector('i');

        if (input.type === 'password') {
            input.type = 'text';
            icon.className = 'fas fa-eye-slash';
        } else {
            input.type = 'password';
            icon.className = 'fas fa-eye';
        }
    }

    // Utility methods
    async copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text);
            View.showSuccess('Copied to clipboard');
            return true;
        } catch (error) {
            console.error('Failed to copy text:', error);
            View.showError('Failed to copy text');
            return false;
        }
    }

    scrollToElement(elementId, offset = 0) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const elementPosition = element.getBoundingClientRect().top + window.pageYOffset;
        const offsetPosition = elementPosition - offset;

        window.scrollTo({
            top: offsetPosition,
            behavior: 'smooth'
        });
    }

    // Cleanup method
    cleanup() {
        const modals = document.querySelectorAll('[id$="Modal"]');
        modals.forEach(modal => modal.remove());

        const notifications = document.querySelectorAll('.notification');
        notifications.forEach(notification => notification.remove());
    }
    
    static showGameListPagination(games) {
    const container = document.getElementById('gameList');
    if (!container) return;

    // Append new games
    games.forEach(game => {
        const gameHTML = `
            <div class="group cursor-pointer" data-game-id="${game.GameId}">
                <div class="relative mb-3">
                    <img src="${game.Poster || 'https://via.placeholder.com/200x300'}" alt="${game.Title}"
                        class="game-poster rounded-lg shadow-lg group-hover:shadow-xl transition-shadow">
                    <div class="absolute inset-0 bg-black bg-opacity-0 group-hover:bg-opacity-20 transition-opacity rounded-lg"></div>
                </div>
                <div class="text-center">
                    <div class="flex justify-center mb-2 text-green-400">
                        ${'★'.repeat(Math.round(game.AvgRating))}${game.AvgRating > 0 ? '½'.repeat(Math.round((game.AvgRating % 1) * 2)) : ''}
                    </div>
                    <div class="flex justify-center space-x-3">
                        <i class="fas fa-layer-group text-${game.AvgRating > 3 ? 'blue-400' : 'gray-600'} text-sm"></i>
                    </div>
                </div>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', gameHTML);
    });
}

static showUserStats(userInfo) {
    const followerEl = document.getElementById('numberFollower');
    const followingEl = document.getElementById('numberFollowing');
    const gamesLoggedEl = document.getElementById('gamesLogged');

    if (followerEl) followerEl.textContent = userInfo.NumberFollower || 0;
    if (followingEl) followingEl.textContent = userInfo.NumberFollowing || 0;
    if (gamesLoggedEl) gamesLoggedEl.textContent = userInfo.GamesLogged || 0;
}

static showFollowers(followers) {
    const container = document.getElementById('followersList');
    if (!container) return;

    container.innerHTML = '';
    followers.forEach(follower => {
        const followerHTML = `
            <div class="text-center">
                <span class="inline-block w-10 h-10 rounded-full bg-gray-600 flex items-center justify-center text-white font-semibold">XX</span>
                <p class="text-sm mt-1">${follower.username}</p>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', followerHTML);
    });
}

static showFollowing(following) {
    const container = document.getElementById('followingList');
    if (!container) return;

    container.innerHTML = '';
    following.forEach(follow => {
        const followHTML = `
            <div class="text-center">
                <span class="inline-block w-10 h-10 rounded-full bg-gray-600 flex items-center justify-center text-white font-semibold">XX</span>
                <p class="text-sm mt-1">${follow.username}</p>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', followHTML);
    });
}

static showHighestRatedGames(reviews) {
    const container = document.getElementById('highestRatedList');
    if (!container) return;

    container.innerHTML = '';
    const sortedReviews = [...reviews].sort((a, b) => b.Rating - a.Rating).slice(0, 10);
    sortedReviews.forEach(review => {
        const stars = '★'.repeat(Math.round(review.Rating)) + '☆'.repeat(5 - Math.round(review.Rating));
        const gameHTML = `
            <div class="flex items-center space-x-3 p-3 bg-gray-800 rounded-lg">
                <img src="${review.Poster || 'https://via.placeholder.com/40x60'}" alt="Game Poster" class="w-8 h-12 rounded">
                <span class="star-rating">${stars}</span>
                <span class="font-medium">${review.Title || review.gameTitle}</span>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', gameHTML);
    });
}

static showRecentlyRatedGames(reviews) {
    const container = document.getElementById('recentlyRatedList');
    if (!container) return;

    container.innerHTML = '';
    const sortedReviews = [...reviews].sort((a, b) => new Date(b.DateCreated) - new Date(a.DateCreated)).slice(0, 10);
    sortedReviews.forEach(review => {
        const stars = '★'.repeat(Math.round(review.Rating)) + '☆'.repeat(5 - Math.round(review.Rating));
        const gameHTML = `
            <div class="flex items-center space-x-3 p-2 bg-gray-800 rounded-lg">
                <img src="${review.Poster || 'https://via.placeholder.com/40x60'}" alt="Game Poster" class="w-6 h-9 rounded">
                <span class="star-rating">${stars}</span>
                <span class="font-medium">${review.Title || review.gameTitle}</span>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', gameHTML);
    });
}

}
