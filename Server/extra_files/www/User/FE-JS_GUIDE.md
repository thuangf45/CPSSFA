screens

### 1. Profile Screen 

**Interactive Features:**
- **Show and Store**: Displays and allows storage of username, avatar, bio, external links, 5 favorite game posters, and 3 currently playing games.
- **Show**: Displays follower count, following count, logged games count, lists count, 5 recent logged games, 3 recent reviews, 3 most liked reviews, and 3 popular lists.
- **Editable**: Allows editing of avatar, username, website links, favorite games, and bio.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/user/profile/{userId}`: Fetches profile data including username, avatar, bio, external links, favorite games, and currently playing games.
  - `PUT /api/user/profile/{userId}`: Updates profile data with new values for avatar, username, website links, favorite games, and bio.
  - `GET /api/user/stats/{userId}`: Retrieves statistics such as follower count, following count, logged games count, and lists count.
  - `GET /api/user/reviews/{userId}`: Fetches recent reviews and most liked reviews.
  - `GET /api/user/lists/{userId}`: Retrieves popular lists created by the user.

- **Database**:
  - `users`: Contains columns: `id` (unique identifier), `username`, `bio`, `external_links` (JSONB for storing links), `avatar`.
  - `games`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `type` (e.g., favorite or currently playing), `order` (for sorting).
  - `game_logs`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `logged_date`.
  - `reviews`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `rating`, `review_text`, `date`, `likes`.
  - `lists`: Contains columns: `id` (unique identifier), `user_id`, `title`, `description`, `date`, `likes`.
  - `followers`: Contains columns: `id` (unique identifier), `follower_id`.

- **Process**:
  1. The client sends a request with a valid authentication token to `GET /api/user/profile/{userId}` to fetch initial profile data.
  2. For statistics, `GET /api/user/stats/{userId}` is called to retrieve counts and related data.
  3. Reviews and lists are fetched using `GET /api/user/reviews/{userId}` and `GET /api/user/lists/{userId}` respectively.
  4. Updates are sent via `PUT /api/user/profile/{userId}` with the modified data, authenticated via token.
  5. The server processes the requests, updating or retrieving data from the respective tables, and returns JSON responses which the client renders.

**Files to Update:**

- **`profile.html`**:
  - Add display/edit sections with IDs: `username`, `bio`, `favoriteGames` for username, bio, and game posters; include stats, logged games, reviews, and lists sections.
  
- **`api.js`**:
  - Enhance `APIUser` with: `GetProfile(userId)`, `PutProfile(userId, data)`, `GetStats(userId)`, `GetReviews(userId)`, `GetLists(userId)`.

- **`handle.js`**:
  - Add: `handleProfileFetch(userId)` (fetches and displays profile data), `handleProfileUpdate(userId, data)` (sends updates).

- **`event.js`**:
  - Add handlers: `profileEdit` and `profileFetch` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render stats, game posters, reviews, and lists dynamically.

---

### 2. Activity Screen 

**Interactive Features:**
- **Show**: Displays a list of activities for the logged-in user and friends.
- **Show**: Provides filter options to narrow activity types.
- **Show**: Offers sort options (Oldest to Newest, Newest to Oldest) to reorder the feed.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/activity/{userId}`: Fetches the activity feed for the user and their friends.
  - `GET /api/activity/filter/{userId}`: Fetches a filtered activity feed based on activity type.

- **Database**:
  - `activities`: Contains columns: `id` (unique identifier), `user_id`, `action_type` (e.g., log, rate, review, follow, block, like, comment, create_list, add_to_list), `target_id` (e.g., game_id, user_id, review_id, list_id), `details` (JSONB for additional data), `created_at`.
  - `users`: Contains columns: `id` (unique identifier), `username`.
  - `games`: Contains columns: `id` (unique identifier), `title`.
  - `reviews`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `rating`, `review_text`, `date`, `likes`.
  - `lists`: Contains columns: `id` (unique identifier), `user_id`, `title`, `description`, `date`, `likes`.
  - `followers`: Contains columns: `id` (unique identifier), `follower_id`, `followed_id`.

- **Process**:
  1. The client sends a request with a valid authentication token to `GET /api/activity/{userId}`.
  2. The server queries the `activities` table, joining with `users`, `games`, `reviews`, and `lists` based on `target_id` and `action_type`.
  3. Filters are applied via `GET /api/activity/filter/{userId}` based on selected activity types.
  4. Results are sorted by `created_at` according to the chosen direction (Oldest to Newest or Newest to Oldest).
  5. The server returns a JSON feed which the client renders dynamically.

**Files to Update:**

- **`activity.html`**:
  - Add activity feed section with ID `activityFeed`, filter dropdown with ID `filterType`, sort selector with ID `sortOrder`, and dynamic list items.

- **`api.js`**:
  - Enhance `APIActivity` with: `GetActivity(userId, filter, sort)`, `GetFilteredActivity(userId, filter, sort)`.

- **`handle.js`**:
  - Add: `handleActivityFetch(userId)` (fetches and displays feed), `handleFilterActivity(filter)` (applies filter), `handleSortActivity(sort)` (applies sort).

- **`event.js`**:
  - Add handlers: `activityFetch`, `filterActivity`, `sortActivity` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render activity feed, update filter/sort UI, and display individual activity items dynamically.

---

### 3. Game Screen

**Interactive Features:**
- **Show**: Displays a list of logged games (with or without ratings/reviews) for the user.
- **Show**: Provides filter options by genre and platform/service.
- **Show**: Offers sort options (Date Logged, Date Published, Average Rating, Your Rating, Title).
- **Show**: Includes a filter for games with reviews only.
- **Show**: Displays icons for liked (heart) and reviewed (3-stack) games.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/games/logged/{userId}`: Fetches logged games for the user.
  - `GET /api/games/filtered/{userId}`: Fetches filtered and sorted logged games.

- **Database**:
  - `game_logs`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `logged_date`, `rating`.
  - `reviews`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `review_text`, `date`.
  - `games`: Contains columns: `id` (unique identifier), `title`, `genre`, `platform`, `release_date`.

- **Process**:
  1. The client sends a request with a valid authentication token to `GET /api/games/logged/{userId}`.
  2. The server queries `game_logs` joined with `games` and `reviews` to include game details and review data.
  3. Filters (genre, platform, reviews only) and sort options (Date Logged, Date Published, Average Rating, Your Rating, Title) are applied via `GET /api/games/filtered/{userId}`.
  4. The server returns a JSON list which the client renders with appropriate icons.

**Files to Update:**

- **`games.html`**:
  - Add logged games grid with IDs: `loggedGames`, filter dropdowns with IDs `genreFilter`, `platformFilter`, `reviewFilter`, sort selector with ID `sortOption`, and icons for liked (heart) and reviewed (3-stack).

- **`api.js`**:
  - Enhance `GameAPI` with: `GetLoggedGames(userId, filter, sort)`, `GetFilteredGames(userId, filter, sort)`.

- **`handle.js`**:
  - Add: `handleGamesFetch(userId)` (fetches logged games), `handleFilterGames(filter)` (applies filters), `handleSortGames(sort)` (applies sort).

- **`event.js`**:
  - Add handlers: `gamesFetch`, `filterGames`, `sortGames` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render game grid, update filter/sort UI, and display liked/reviewed icons dynamically.



### 4. Lists Screen

**Interactive Features:**
- **Show**: Displays a grid of user-created lists, each with a title, description, total number of games, creation date, and like count (heart icon).
- **Show**: Includes a search bar for searching games, located at the top right.
- **Show**: Provides sort options by Date Created, Title, and Number of Games, with a dropdown selector and a sort direction toggle (Newest First/Oldest First).
- **Show**: Displays a "Create New List" button to initiate the creation of a new list.
- **Show**: Shows game names and posters within each list preview, limited to a subset (e.g., three games) with a placeholder background and game count overlay.
- **Show**: Highlights liked lists with a heart icon and like count per list.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/lists/{userId}`: Fetches all lists created by the specified user. Returns a JSON array of list objects, including `id`, `title`, `description`, `dateCreated`, `likes`, `gameCount`, and an array of game details (e.g., `game_id`, `title`, `poster`).
  - `GET /api/lists/{listId}/games`: Retrieves detailed game information (e.g., `title`, `poster`) for a specific list. Returns a JSON array of game objects associated with the list.

- **Database**:
  - `lists`: Stores user-created lists with columns: `id` (unique identifier for the list), `user_id` (foreign key referencing the user), `title` (list title), `description` (list description), `dateCreated` (date the list was created), `likes` (number of likes), `gameCount` (total number of games).
  - `list_games`: Junction table linking lists to games with columns: `list_id` (foreign key referencing the list), `game_id` (foreign key referencing the game).
  - `games`: Stores game metadata with columns: `id` (unique identifier for the game), `title` (game title), `poster` (URL or path to the game poster image).

- **Process**:
  1. The client sends a request with a valid authentication token stored in `localStorage` (managed by `API.setToken` and `API.getToken` in `api.js`) to `GET /api/lists/{userId}`.
  2. The server queries the relevant data to fetch all lists for the user, including game titles and posters for each list.
  3. For each list, the server limits the returned game data to a subset (e.g., first three games) and includes the total `gameCount`.
  4. The response is processed to render the list grid, applying the selected sort option (e.g., `dateCreated`, `title`, or `gameCount`) and direction (Newest First/Oldest First).
  5. For detailed game previews, an additional `GET /api/lists/{listId}/games` request can be made if needed, though initial rendering uses the embedded game data from the first endpoint.
  6. The client updates the UI with the fetched data, displaying titles, descriptions, game names, posters, and like counts.

**Files to Update:**

- **`lists.html`**:
  - Ensure the grid container with ID `listsContainer` displays list cards, each showing `title`, `description`, `gameCount`, `dateCreated`, and `likes` with a heart icon.
  - Include a sort dropdown with ID `sortOption` for selecting Date Created, Title, or Number of Games, and a direction toggle with ID `sortDirection` (e.g., Newest First).
  - Add a search input with ID `gameSearch` for game searching and a "Friends and People" button.
  - Update each list card to display game names (e.g., `Game 1`, `Game 2`, `Game 3`) and placeholder posters within a `w-80 h-32` div, overlaid with a `gameCount` badge.
  - Include a "Create New List" button with ID `createListBtn`.

- **`api.js`**:
  - Enhance the `ListAPI` class with:
    - `GetLists(userId, sort, direction)`: Sends a `GET` request to `/api/lists/{userId}` with query parameters for sorting (e.g., `dateCreated`, `title`, `gameCount`) and direction (e.g., `asc`, `desc`).
    - `GetListGames(listId)`: Sends a `GET` request to `/api/lists/{listId}/games` to fetch detailed game data (titles and posters) for a specific list.
  - Use `API.buildQuery` to construct query strings and include token authentication via `X_Token_Authorization` headers.

- **`handle.js`**:
  - Add the following methods to the `Handle` class:
    - `handleListsFetch(userId)`: Calls `ListAPI.GetLists` to fetch user lists, displays a loading indicator via `UI.showLoading`, and renders the results with `UI.addListToPage`.
    - `handleSortLists(sort, direction)`: Calls `ListAPI.GetLists` with updated sort parameters and re-renders the list grid.
    - `handleCreateList()`: Initiates the creation process, potentially showing a modal via `UI.showModal`.

- **`event.js`**:
  - Add event handlers in `document.addEventListener('DOMContentLoaded', ...)`:
    - `listenIfExists('#sortOption', 'change', () => Handle.handleSortLists(document.getElementById('sortOption').value, document.getElementById('sortDirection').value))`.
    - `listenIfExists('#sortDirection', 'change', () => Handle.handleSortLists(document.getElementById('sortOption').value, document.getElementById('sortDirection').value))`.
    - `listenIfExists('#createListBtn', 'click', () => Handle.handleCreateList())`.
    - `listenIfExists('#gameSearch', 'input', () => Handle.handleSearchGames(document.getElementById('gameSearch').value))`.

- **`ui.js`**:
  - Add methods to the `UIManager` class:
    - `addListToPage(listData)`: Renders a list card with `title`, `description`, `gameCount`, `dateCreated`, `likes`, and a subset of game names/posters.
    - `sortListsInDOM(sortBy)`: Reorganizes the `listsContainer` based on the selected sort option and direction.
    - `updateListLikes(listId, newLikes)`: Updates the like count display for a specific list card.
    - `displayGameSearchResults(results)`: Renders search results in a separate container if triggered by the search input.



### 5. Diary Screen 
**Interactive Features:**
- **Show**: Displays a chronological list of games logged by the user, organized by month (e.g., July 2025, June 2025), with each entry showing the game title, release year, rating (e.g., ★★★★½), and icons for liked (❤️) or replayed (🔁) status.
- **Show**: Includes a "Friends and People" button for social navigation.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/diary/{userId}`: Fetches the logged games for the specified user, organized by month. Returns a JSON array of objects containing `date`, `game_id`, `title`, `release_year`, `rating`, and status flags (e.g., `liked`, `replayed`).

- **Database**:
  - `game_logs`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `logged_date`, `rating`, `liked` (boolean), `replayed` (boolean).
  - `games`: Contains columns: `id` (unique identifier), `title`, `release_year`.

- **Process**:
  1. The client sends a request with a valid authentication token to `GET /api/diary/{userId}`.
  2. The server queries the `game_logs` table, joining with `games` to retrieve game titles and release years, and groups results by month based on `logged_date`.
  3. The server returns a JSON response with monthly sections, each containing game entries with ratings and status flags.
  4. The client renders the diary with collapsible monthly sections and dynamic icons based on `liked` and `replayed` statuses.

**Files to Update:**

- **`diary.html`**:
  - Ensure a container with ID `diaryContainer` displays monthly sections (e.g., July 2025) with game entries showing `title`, `release_year`, `rating`, and icons (❤️, 🔁).
  - Include a "Friends and People" button.

- **`api.js`**:
  - Enhance `DiaryAPI` with: `GetDiary(userId)` to fetch logged games.

- **`handle.js`**:
  - Add: `handleDiaryFetch(userId)` (fetches and displays diary data).

- **`event.js`**:
  - Add handler: `diaryFetch` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render monthly diary sections, display game entries with ratings and icons dynamically.

---

### 6. Likes Screen 

**Interactive Features:**
- **Show**: Displays a list of items the user has liked, including games, reviews, and comments, with details like title, rating, like count, and date liked.
- **Show**: Provides filter options for All Types, Games, Reviews, and Comments, with a dropdown selector.
- **Show**: Offers sort options (Newest to Oldest, Oldest to Newest) with a sort selector.
- **Show**: Includes a "Load More Likes" button for pagination.
- **Show**: Displays user ratings (e.g., 5/5) and tags (e.g., #crpg, #masterpiece) for liked items.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/likes/{userId}`: Fetches all liked items (games, reviews, comments) for the user. Returns a JSON array with `type` (game, review, comment), `id`, `title` or `content`, `rating`, `likes`, `date_liked`, and additional metadata (e.g., tags).
  - `GET /api/likes/filtered/{userId}`: Fetches filtered and sorted liked items based on type and date. Returns a filtered JSON array.

- **Database**:
  - `likes`: Contains columns: `id` (unique identifier), `user_id`, `type` (game, review, comment), `target_id`, `date_liked`.
  - `games`: Contains columns: `id` (unique identifier), `title`, `rating`.
  - `reviews`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `rating`, `review_text`, `likes`, `tags` (JSONB).
  - `comments`: Contains columns: `id` (unique identifier), `user_id`, `target_id` (e.g., review_id), `content`, `likes`.

- **Process**:
  1. The client sends a request with a valid authentication token to `GET /api/likes/{userId}`.
  2. The server queries the `likes` table, joining with `games`, `reviews`, or `comments` based on `type` and `target_id` to fetch detailed data.
  3. Filters (All Types, Games, Reviews, Comments) and sort options (Newest to Oldest, Oldest to Newest) are applied via `GET /api/likes/filtered/{userId}`.
  4. The server returns a paginated JSON response, with a "Load More" trigger based on the number of items.
  5. The client renders the list with dynamic content, including ratings, tags, and a "Load More Likes" button.

**Files to Update:**

- **`likes.html`**:
  - Ensure a container with ID `likesContainer` displays liked items (games, reviews, comments) with IDs like `likeItem`, filter dropdown with ID `filterType`, sort selector with ID `sortOrder`, and a "Load More Likes" button.
  - Include user ratings, like counts, and tags within each item.

- **`api.js`**:
  - Enhance a new `LikesAPI` class with: `GetLikes(userId, filter, sort)`, `GetFilteredLikes(userId, filter, sort)`.

- **`handle.js`**:
  - Add: `handleLikesFetch(userId)` (fetches and displays liked items), `handleFilterLikes(filter)` (applies filter), `handleSortLikes(sort)` (applies sort), `handleLoadMoreLikes(userId)` (loads additional items).

- **`event.js`**:
  - Add handlers: `likesFetch`, `filterLikes`, `sortLikes`, `loadMoreLikes` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render liked items (games, reviews, comments), update filter/sort UI, display ratings, tags, and handle pagination with "Load More Likes".

---

### 7. Play Later List Screen 

**Interactive Features:**
- **Show**: Displays a list of games the user has added to their "Play Later" list, with game titles.
- **Show**: Provides filter options by genre (e.g., All Genres, Action, Adventure, RPG) and platform/service (e.g., All Platforms, Steam, Nintendo Switch).
- **Show**: Offers sort options (Date Logged, Date Published, Average Rating, Your Rating, Title) with a sort selector.
- **Show**: Includes a "Load More Games" button for pagination.
- **Show**: Includes a "Friends and People" button for social navigation.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/playlater/{userId}`: Fetches the "Play Later" list for the user. Returns a JSON array of game objects with `id`, `title`, `genre`, `platform`, `logged_date`, and `rating`.
  - `GET /api/playlater/filtered/{userId}`: Fetches filtered and sorted "Play Later" games based on genre, platform, and sort criteria. Returns a filtered JSON array.

- **Database**:
  - `playlater`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `logged_date`, `rating`.
  - `games`: Contains columns: `id` (unique identifier), `title`, `genre`, `platform`.

- **Process**:
  1. The client sends a request with a valid authentication token to `GET /api/playlater/{userId}`.
  2. The server queries the `playlater` table, joining with `games` to retrieve game details.
  3. Filters (genre, platform) and sort options (Date Logged, Date Published, Average Rating, Your Rating, Title) are applied via `GET /api/playlater/filtered/{userId}`.
  4. The server returns a paginated JSON response, with a "Load More Games" trigger based on the number of items.
  5. The client renders the list with game titles and a "Load More Games" button.

**Files to Update:**

- **`playlaterlist.html`**:
  - Ensure a container with ID `playLaterContainer` displays game titles, filter dropdowns with IDs `genreFilter` and `platformFilter`, sort selector with ID `sortOption`, and a "Load More Games" button.
  - Include a "Friends and People" button.

- **`api.js`**:
  - Enhance a new `PlayLaterAPI` class with: `GetPlayLater(userId, filter, sort)`, `GetFilteredPlayLater(userId, filter, sort)`.

- **`handle.js`**:
  - Add: `handlePlayLaterFetch(userId)` (fetches and displays "Play Later" games), `handleFilterPlayLater(filter)` (applies filters), `handleSortPlayLater(sort)` (applies sort), `handleLoadMorePlayLater(userId)` (loads additional games).

- **`event.js`**:
  - Add handlers: `playLaterFetch`, `filterPlayLater`, `sortPlayLater`, `loadMorePlayLater` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render "Play Later" game list, update filter/sort UI, and handle pagination with "Load More Games".

### 8. Friends Screen 

**Interactive Features:**
- **Show**: Displays sections for Popular games, Friend Activities, Popular with Friends, and a list of Your Friends, each showing game titles with optional ratings (e.g., ★★★★★).
- **Show**: Includes a "Friends and People" button for social navigation and a description encouraging discovery of trends and friend activities.
- **Show**: Highlights games with ratings and dynamic updates based on friend interactions.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/friends/popular/{userId}`: Fetches a list of popular games. Returns a JSON array of game objects with `id`, `title`.
  - `GET /api/friends/activities/{userId}`: Fetches friend activities, including games and ratings. Returns a JSON array with `friend_id`, `game_id`, `title`, `rating`.
  - `GET /api/friends/popularWithFriends/{userId}`: Fetches games popular among friends. Returns a JSON array of game objects with `id`, `title`.
  - `GET /api/friends/list/{userId}`: Fetches the list of friends with their current or recent game activity. Returns a JSON array with `friend_id`, `username`, `game_id`, `title`, `activity_type` (e.g., playing, reviewed).

- **Database**:
  - `games`: Contains columns: `id` (unique identifier), `title`.
  - `friend_activities`: Contains columns: `id` (unique identifier), `user_id`, `friend_id`, `game_id`, `rating`, `activity_date`.
  - `friends`: Contains columns: `id` (unique identifier), `user_id`, `friend_id`, `status`.
  - `users`: Contains columns: `id` (unique identifier), `username`.

- **Process**:
  1. The client sends a request with a valid authentication token to `GET /api/friends/popular/{userId}`.
  2. The server queries popular games, friend activities, and popular games among friends, joining with `friends` and `users` for friend data.
  3. Friend list and activities are fetched via `GET /api/friends/list/{userId}` and `GET /api/friends/activities/{userId}` respectively.
  4. The server returns JSON responses which the client renders into sections with dynamic ratings and friend details.

**Files to Update:**

- **`friends.html`**:
  - Ensure containers with IDs `popularGames`, `friendActivities`, `popularWithFriends`, and `yourFriends` display respective game and friend data with optional ratings.
  - Include a "Friends and People" button.

- **`api.js`**:
  - Enhance `FriendsAPI` with: `GetPopular(userId)`, `GetActivities(userId)`, `GetPopularWithFriends(userId)`, `GetFriendsList(userId)`.

- **`handle.js`**:
  - Add: `handleFriendsFetch(userId)` (fetches and displays all friend-related data).

- **`event.js`**:
  - Add handler: `friendsFetch` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render popular games, friend activities, popular with friends, and friend list with ratings dynamically.

---

### 9. Game Detail Screen

**Interactive Features:**
- **Show**: Displays game details including title, average rating, review count, genres, available services, studio, publisher, release date, country, languages, and description.
- **Show**: Includes sections for Reviews, Details, Genres, and Services with collapsible tabs.
- **Show and Editable**: Allows rating (1-10 stars), liking (♥), logging date, and adding to lists or PlayLaterList with "Cancel" and "Save" buttons.
- **Show**: Displays user-specific actions like Rate & Log, Write Review, Add to Lists, and Add to PlayLaterList.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/game/{gameId}`: Fetches game details including title, rating, genres, services, etc. Returns a JSON object with `id`, `title`, `average_rating`, `review_count`, `genres`, `services`, `studio`, `publisher`, `release_date`, `country`, `languages`, `description`.
  - `GET /api/game/reviews/{gameId}`: Fetches reviews for the game. Returns a JSON array with `user_id`, `username`, `rating`, `review_text`, `date`, `tags`.
  - `PUT /api/game/user/{userId}/{gameId}`: Updates user-specific data (rating, like, log date, list additions). Returns a success status.
  - `POST /api/game/playlater/{userId}/{gameId}`: Adds game to PlayLaterList. Returns a success status.

- **Database**:
  - `games`: Contains columns: `id` (unique identifier), `title`, `average_rating`, `review_count`, `genres` (JSONB), `services` (JSONB), `studio`, `publisher`, `release_date`, `country`, `languages` (JSONB), `description`.
  - `reviews`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `rating`, `review_text`, `date`, `tags` (JSONB).
  - `game_logs`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `logged_date`, `rating`, `liked`.
  - `playlater`: Contains columns: `id` (unique identifier), `user_id`, `game_id`.
  - `lists`: Contains columns: `id` (unique identifier), `user_id`, `title`, `description`, `game_ids` (JSONB).

- **Process**:
  1. The client sends a request with a token to `GET /api/game/{gameId}` for initial game data.
  2. Reviews are fetched via `GET /api/game/reviews/{gameId}`.
  3. User actions (rating, liking, logging) are sent via `PUT /api/game/user/{userId}/{gameId}` with updated data.
  4. Adding to PlayLaterList or lists triggers `POST /api/game/playlater/{userId}/{gameId}` or updates `lists` table.
  5. The server returns JSON responses, and the client updates the UI with tabs and action buttons.

**Files to Update:**

- **`game-detail.html`**:
  - Ensure containers with IDs `gameDetails`, `reviewsSection`, `genresSection`, `servicesSection` display respective data; include rating stars with ID `ratingStars`, like button with ID `likeButton`, and list modals with IDs `addToListsModal`, `playLaterButton`.

- **`api.js`**:
  - Enhance `GameAPI` with: `GetGame(gameId)`, `GetGameReviews(gameId)`, `PutGameUser(userId, gameId, data)`, `PostPlayLater(userId, gameId)`.

- **`handle.js`**:
  - Add: `handleGameDetailsFetch(gameId)` (fetches and displays game data), `handleGameUpdate(userId, gameId, data)` (saves user actions), `handleAddToPlayLater(userId, gameId)` (adds to PlayLaterList).

- **`event.js`**:
  - Add handlers: `gameDetailsFetch`, `gameUpdate`, `addToPlayLater` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render game details, reviews, genres, services, handle rating/like UI, and manage list additions dynamically.

---

### 10. Game Review Screen 

**Interactive Features:**
- **Show**: Displays a specific game review with author, date, game title, rating, review text, tags, and like count.
- **Show**: Includes a "Back" button and a "Post Comment" section with a button.
- **Show**: Displays comments with author, date, content, like count, and a Reply button.
- **Show**: Allows liking the review with a like button.

**Backend/Database Interaction:**

- **Endpoints**:
  - `GET /api/review/{reviewId}`: Fetches a specific review. Returns a JSON object with `user_id`, `username`, `game_id`, `title`, `rating`, `review_text`, `date`, `tags`, `likes`.
  - `GET /api/review/comments/{reviewId}`: Fetches comments for the review. Returns a JSON array with `user_id`, `username`, `content`, `date`, `likes`.
  - `POST /api/review/like/{userId}/{reviewId}`: Increments the like count for the review. Returns a success status.
  - `POST /api/review/comment/{userId}/{reviewId}`: Adds a new comment. Returns a success status.

- **Database**:
  - `reviews`: Contains columns: `id` (unique identifier), `user_id`, `game_id`, `rating`, `review_text`, `date`, `tags` (JSONB), `likes`.
  - `comments`: Contains columns: `id` (unique identifier), `user_id`, `review_id`, `content`, `date`, `likes`.
  - `games`: Contains columns: `id` (unique identifier), `title`.
  - `users`: Contains columns: `id` (unique identifier), `username`.

- **Process**:
  1. The client sends a request with a token to `GET /api/review/{reviewId}` for review data.
  2. Comments are fetched via `GET /api/review/comments/{reviewId}`.
  3. Liking the review triggers `POST /api/review/like/{userId}/{reviewId}`.
  4. Posting a comment uses `POST /api/review/comment/{userId}/{reviewId}` with the comment text.
  5. The server updates the database and returns JSON responses, which the client renders with dynamic like counts and comments.

**Files to Update:**

- **`game-review.html`**:
  - Ensure a container with ID `reviewContainer` displays review details (author, date, title, rating, text, tags, likes) with ID `likeButton`; include comments section with ID `commentsSection` and a "Post Comment" button with ID `postCommentBtn`.

- **`api.js`**:
  - Enhance `ReviewAPI` with: `GetReview(reviewId)`, `GetReviewComments(reviewId)`, `PostReviewLike(userId, reviewId)`, `PostReviewComment(userId, reviewId, content)`.

- **`handle.js`**:
  - Add: `handleReviewFetch(reviewId)` (fetches and displays review), `handleLikeReview(userId, reviewId)` (increments like), `handlePostComment(userId, reviewId, content)` (adds comment).

- **`event.js`**:
  - Add handlers: `reviewFetch`, `likeReview`, `postComment` in `handleGlobalClick`.

- **`ui.js`**:
  - Add methods: Render review details, comments, handle like button, and manage comment posting dynamically.

### 11. Reviews Screen

**Interactive Features:**
- **Show**: Displays a list of user reviews with titles, ratings, dates, likes, content, tags, and action buttons (Edit, Delete, Share).
- **Show**: Provides filter options by genre and platform.
- **Show**: Offers sort options (Date, Rating, Likes, Title).
- **Show**: Includes a search functionality to filter reviews by title, content, or tags.
- **Show**: Allows adding new reviews via a modal with title, rating (star-based), review text, and tags.
- **Show**: Supports editing existing reviews with the same modal fields.
- **Show**: Enables deleting reviews with confirmation.
- **Show**: Supports sharing reviews via native share API or clipboard.
- **Show**: Features a "Load More" button for pagination.
- **Show**: Displays a "No Results" message when filters yield no matches.

**Backend/Database Interaction:**

- **Endpoints:**
  - `GET /api/reviews/{userId}`: Fetches all reviews for the user.
  - `GET /api/reviews/filtered/{userId}`: Fetches filtered and sorted reviews based on genre, platform, search term, and sort criteria.
  - `POST /api/reviews/{userId}`: Adds a new review.
  - `PUT /api/reviews/{userId}/{reviewId}`: Updates an existing review.
  - `DELETE /api/reviews/{userId}/{reviewId}`: Deletes a review.
  - `POST /api/reviews/like/{userId}/{reviewId}`: Increments the like count for a review.

- **Database:**
  - `reviews`: Contains columns: `id` (unique identifier), `user_id`, `title`, `rating`, `date`, `likes`, `content`, `tags` (JSON array), `genre`, `platform`, `poster_url`.

- **Process:**
  1. The client sends a request with a valid authentication token to `GET /api/reviews/{userId}` to fetch initial reviews.
  2. The server queries the `reviews` table for records matching `user_id`.
  3. For filtered views, the client sends a request to `GET /api/reviews/filtered/{userId}` with query parameters (genre, platform, search, sort).
  4. The server applies filters and sorting, returning a paginated JSON list.
  5. For adding/updating/deleting, the client sends `POST`, `PUT`, or `DELETE` requests with review data, and the server updates the `reviews` table accordingly.
  6. For likes, the client sends a `POST` to `POST /api/reviews/like/{userId}/{reviewId}`, and the server increments the `likes` column.
  7. The server returns success responses or updated data, which the client renders or updates in the UI.

**Files to Update:**

- **`reviews.html`**:
  - Update the reviews list container with ID `reviewsList`, filter dropdowns with IDs `genreFilter`, `platformFilter`, sort selector with ID `sortFilter`, search input with ID `searchInput`, "Load More" button with ID `loadMoreBtn`, "No Results" message with ID `noResults`, and modal with ID `addReviewModal` for adding/editing reviews.

- **`api.js`**:
  - Enhance `GameAPI` with: 
    - `GetReviews(userId)`: Fetches all reviews.
    - `GetFilteredReviews(userId, filters, sort)`: Fetches filtered and sorted reviews.
    - `PostReview(userId, reviewData)`: Adds a new review.
    - `PutReview(userId, reviewId, reviewData)`: Updates an existing review.
    - `DeleteReview(userId, reviewId)`: Deletes a review.
    - `PostReviewLike(userId, reviewId)`: Increments the like count.

- **`handle.js`**:
  - Add: 
    - `handleReviewsFetch(userId)`: Fetches and initializes reviews.
    - `handleFilterReviews(filters)`: Applies filters to reviews.
    - `handleSortReviews(sort)`: Applies sorting to reviews.
    - `handleAddReview(userId, reviewData)`: Submits a new review.
    - `handleUpdateReview(userId, reviewId, reviewData)`: Updates an existing review.
    - `handleDeleteReview(userId, reviewId)`: Deletes a review.
    - `handleLikeReview(userId, reviewId)`: Toggles the like count.

- **`event.js`**:
  - Add handlers: 
    - `reviewsFetch` in `DOMContentLoaded` to initialize reviews.
    - `filterReviews` for `change` events on `genreFilter`, `platformFilter`, `sortFilter`.
    - `searchReviews` for `input` event on `searchInput`.
    - `loadMoreReviews` for `click` event on `loadMoreBtn`.
    - `addReview` for `click` on `addReviewBtn`.
    - `editReview` for `click` on `.edit-review-btn`.
    - `deleteReview` for `click` on `.delete-review-btn`.
    - `shareReview` for `click` on `.share-review-btn`.
    - `likeReview` for `click` on `.like-btn`.

- **`ui.js`**:
  - Add methods: 
    - `renderReviews(reviews)`: Renders the review list in `reviewsList`.
    - `updateReviewStats(total)`: Updates the total reviews count.
    - `showNoResults()`: Displays the "No Results" message.
    - `showLoading()`: Shows the loading spinner on `loadMoreBtn`.
    - `hideLoading()`: Hides the loading spinner.
    - `openModal(reviewData)`: Populates and opens the `addReviewModal` for adding/editing.
    - `closeModal()`: Closes the modal and resets it.
    - `updateLikeCount(reviewId, likes)`: Updates the like count display for a review.