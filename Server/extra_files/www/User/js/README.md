# Hướng Dẫn Sử Dụng Các File: `event.js`, `api.js`, `handle.js`, và `ui.js`

Bốn file `event.js`, `api.js`, `handle.js`, và `ui.js` là các thành phần cốt lõi của ứng dụng web, mỗi file đảm nhận một vai trò cụ thể trong việc xử lý sự kiện, giao tiếp API, logic nghiệp vụ, và quản lý giao diện người dùng (UI). Tài liệu này cung cấp mô tả chi tiết về mục đích, cấu trúc, cách sử dụng, và hướng dẫn chỉnh sửa/thêm/xóa các hàm trong từng file.

## 1. File `event.js`

### 1.1. Mục Đích
- **Khởi tạo sự kiện**: Thiết lập các sự kiện (event listeners) cho các phần tử HTML như nút, form, hoặc các thành phần tương tác khác để phản hồi hành động của người dùng.
- **Kết nối giao diện và logic**: Kết nối các hành động của người dùng (như click nút đăng nhập) với các hàm xử lý trong `handle.js`.
- **Quản lý loading ban đầu**: Hiển thị hiệu ứng loading khi trang được tải để cải thiện trải nghiệm người dùng.
- **Lắng nghe sự kiện toàn cục**: File này được chèn vào các file HTML để lắng nghe mọi sự kiện tương tác của người dùng.

### 1.2. Cấu Trúc
- **Cách tích hợp vào HTML**:
  - File `event.js` được chèn vào cuối thẻ `<body>` trong các file HTML bằng cách sử dụng:
    ```html
    <script type="module" src="./js/event.js"></script>
    ```
  - Việc sử dụng `type="module"` đảm bảo hỗ trợ ES Modules, cho phép import/export các thành phần từ `handle.js` và `ui.js`.
- **Import**:
  ```javascript
  import { Handle } from './handle.js';
  import { UIManager } from './ui.js';
  ```
  - `Handle`: Chứa các hàm xử lý logic nghiệp vụ (ví dụ: `Login`, `Register`, `Logout`).
  - `UIManager`: Quản lý giao diện người dùng (hiển thị thông báo, loading, modal, v.v.).
- **Khởi tạo UI**:
  ```javascript
  const UI = new UIManager();
  ```
  - Tạo instance của `UIManager` để sử dụng trong toàn bộ file.
- **Tiện ích**:
  - `listenIfExists(selector, event, handler)`: Thêm sự kiện cho phần tử HTML nếu nó tồn tại.
    - **Tham số**:
      - `selector`: Chuỗi bắt đầu bằng `#` + ID của phần tử HTML (ví dụ: `#button-auth`).
      - `event`: Loại sự kiện (xem danh sách bên dưới).
      - `handler`: Hàm xử lý sự kiện, thường gọi các phương thức trong `Handle`.
    - **Ví dụ**:
      ```javascript
      listenIfExists('#button-auth', 'click', () => {
          const username = document.getElementById('loginUsername')?.value || '';
          const password = document.getElementById('loginPassword')?.value || '';
          Handle.Login(username, password);
      });
      ```
  - `listenWindow(event, handler)`: Thêm sự kiện cho đối tượng `window`.
    - **Tham số**:
      - `event`: Loại sự kiện (xem danh sách bên dưới).
      - `handler`: Hàm xử lý sự kiện.
    - **Ví dụ**:
      ```javascript
      listenWindow('load', () => {
          UI.showLoading();
          setTimeout(() => UI.hideLoading(), 2000);
      });
      ```
- **Danh sách loại sự kiện (event types)**:
  - `click`: Khi người dùng nhấp chuột vào phần tử.
  - `input`: Khi giá trị của phần tử input thay đổi.
  - `change`: Khi giá trị của phần tử select, checkbox, hoặc radio thay đổi.
  - `submit`: Khi form được gửi đi (dùng cho `<form>`).
  - `keydown`, `keypress`, `keyup`: Các sự kiện liên quan đến phím bấm.
  - `mouseover`, `mouseout`: Khi chuột di chuyển vào/ra khỏi phần tử.
  - `focus`, `blur`: Khi phần tử được tập trung hoặc mất tập trung.
  - `load`: Khi trang hoặc tài nguyên được tải xong (thường dùng với `window`).
  - `resize`: Khi cửa sổ trình duyệt thay đổi kích thước.
  - `scroll`: Khi người dùng cuộn trang.
  - Các sự kiện khác: Xem tài liệu [MDN Web Events](https://developer.mozilla.org/en-US/docs/Web/Events) để biết thêm.
- **Sự kiện chính**:
  - Được thiết lập trong `DOMContentLoaded` để xử lý các hành động như đăng nhập, đăng ký, đăng xuất, tải profile.
  - **Ví dụ**:
    ```javascript
    document.addEventListener('DOMContentLoaded', () => {
        listenIfExists('#button-auth', 'click', () => {
            const username = document.getElementById('loginUsername')?.value || '';
            const password = document.getElementById('loginPassword')?.value || '';
            Handle.Login(username, password);
        });
    });
    ```

### 1.3. Cách Sử Dụng
- **Thêm sự kiện mới**:
  - Thêm vào block `DOMContentLoaded` sử dụng `listenIfExists` hoặc `listenWindow`.
  - **Ví dụ**: Thêm sự kiện cho nút xem danh sách bạn bè:
    ```javascript
    listenIfExists('#view-friends', 'click', () => {
        Handle.LoadFriends('user123');
    });
    ```
- **Chỉnh sửa sự kiện**:
  - Tìm và sửa handler trong `listenIfExists` hoặc `listenWindow`.
  - Đảm bảo ID phần tử HTML tồn tại và hàm trong `Handle` hợp lệ.
- **Xóa sự kiện**:
  - Xóa dòng `listenIfExists` hoặc `listenWindow` liên quan.
  - Kiểm tra xem hàm trong `Handle` có còn được sử dụng ở nơi khác không.

### 1.4. Lưu Ý
- Đảm bảo các ID phần tử HTML (ví dụ: `#button-auth`) tồn tại trong DOM.
- Sử dụng đúng loại sự kiện phù hợp với hành động (ví dụ: `click` cho nút, `submit` cho form).
- Chèn `<script type="module" src="./js/event.js"></script>` vào cuối thẻ `<body>` trong mọi file HTML cần lắng nghe sự kiện.

---

## 2. File `api.js`

### 2.1. Mục Đích
- **Giao tiếp với server**: Cung cấp các phương thức để gửi yêu cầu HTTP (`GET`, `POST`, `PUT`, `DELETE`) tới các API endpoints.
- **Quản lý token**: Tự động thêm token vào header và lưu token mới từ response.
- **Xây dựng query string**: Tạo query string từ object tham số để gửi kèm yêu cầu.
- **Đóng gói thao tác server**: Cho phép gọi các API bất đồng bộ (dùng `await`) một cách đơn giản.

### 2.2. Cấu Trúc
- **Class chính**:
  - `API`: Class cơ sở với các phương thức HTTP (`GET`, `POST`, `PUT`, `DELETE`) và tiện ích như `buildQuery`, `getToken`, `setToken`.
  - `APIAuth`: Xử lý đăng nhập, đăng xuất, xác thực token.
  - `APIUser`: Quản lý thông tin người dùng (tạo, cập nhật, xóa).
  - `ReviewAPI`: Quản lý đánh giá game.
  - `GameAPI`: Quản lý thông tin game.
  - `CacheAPI`: Quản lý cache dữ liệu.
- **Ví dụ phương thức**:
  - Đăng nhập:
    ```javascript
    async PostLogin(username, password) {
        const payload = { username, password };
        const query = API.buildPathWithQuery('login');
        const res = await this.POST(query, {
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        return res;
    }
    ```

### 2.3. Cách Sử Dụng
- **Gọi API**:
  - Tạo instance của class tương ứng và gọi API với `await`:
    ```javascript
    const api = new APIAuth();
    const res = await api.PostLogin('username', 'password');
    if (res.ok) {
        console.log('Đăng nhập thành công:', res.data);
    } else {
        console.error('Đăng nhập thất bại:', res.data);
    }
    ```
- **Xử lý token**:
  - Token được tự động thêm vào header `X_Token_Authorization` nếu tồn tại trong `localStorage`.
  - Token mới từ response được lưu qua `API.setToken`.
- **Thêm API mới**:
  - Thêm phương thức vào class phù hợp (ví dụ: `APIUser`).
  - **Ví dụ**: Lấy danh sách bạn bè:
    ```javascript
    async GetFriends(userId) {
        const query = API.buildQuery({ userId });
        const res = await this.GET(query);
        return res;
    }
    ```
- **Xóa API**:
  - Xóa phương thức và kiểm tra xem có hàm nào trong `handle.js` hoặc `event.js` gọi đến không.

### 2.4. Lưu Ý
- Đảm bảo `baseUrl` trong mỗi class con được đặt đúng (ví dụ: `/api/auth`, `/api/user`).
- Luôn kiểm tra `res.ok` để xử lý lỗi.
- Sử dụng `API.buildQuery` để tạo query string an toàn.

---

## 3. File `handle.js`

### 3.1. Mục Đích
- **Trung tâm điều phối**: Xử lý logic nghiệp vụ, điều phối giữa các sự kiện được kích hoạt từ `event.js`, gọi API từ `api.js`, và cập nhật giao diện qua `ui.js`.
- **Xác thực và quản lý người dùng**: Xử lý các hành động như đăng nhập, đăng ký, đăng xuất, đảm bảo trải nghiệm mượt mà.
- **Tích hợp API và UI**: Sử dụng các API để lấy hoặc gửi dữ liệu và cập nhật giao diện người dùng tương ứng.

### 3.2. Cấu Trúc
- **Class `Handle`**:
  - Chứa các phương thức tĩnh như `Login`, `Register`, `Logout`.
  - **Ví dụ**:
    ```javascript
    static async Login(username, password) {
        if (!username || !password) {
            UI.showWarning("Vui lòng nhập đầy đủ thông tin!");
            return false;
        }
        const api = new APIAuth();
        UI.showLoading();
        const res = await api.PostLogin(username, password);
        UI.hideLoading();
        if (!res.ok) {
            UI.showWarning("Tài khoản hoặc mật khẩu không đúng!");
            return false;
        }
        UI.showSuccess("Đăng nhập thành công!");
        UI.goTo(Pages.PROFILE);
        return true;
    }
    ```

### 3.3. Cách Sử Dụng
- **Gọi hàm xử lý**:
  - Sử dụng trực tiếp các phương thức tĩnh từ `event.js`:
    ```javascript
    Handle.Login('username', 'password');
    ```
- **Thêm hàm mới**:
  - Thêm phương thức tĩnh vào `Handle`, tích hợp với API và UI.
  - **Ví dụ**: Xử lý tải danh sách bạn bè:
    ```javascript
    static async LoadFriends(userId) {
        const api = new APIUser();
        UI.showLoading();
        const res = await api.GetFriends(userId);
        UI.hideLoading();
        if (res.ok) {
            UI.displayFriendsList(res.data.friends);
            UI.showSuccess("Tải danh sách bạn bè thành công!");
        } else {
            UI.showWarning("Không thể tải danh sách bạn bè!");
        }
    }
    ```
- **Xóa hàm**:
  - Xóa phương thức và kiểm tra các sự kiện trong `event.js` có gọi đến hàm đó không.

### 3.4. Lưu Ý
- Luôn kiểm tra đầu vào trước khi gọi API để tránh lỗi.
- Sử dụng `UI.showLoading` và `UI.hideLoading` để hiển thị trạng thái xử lý.
- Đảm bảo gọi `UI.showSuccess` hoặc `UI.showWarning` để thông báo kết quả.

---

## 4. File `ui.js`

### 4.1. Mục Đích
- **Quản lý giao diện người dùng**: Cung cấp các hàm để tạo hiệu ứng giao diện nhanh gọn như hiển thị thông báo, loading, modal, danh sách, và xác thực đầu vào.
- **Điều hướng trang**: Chuyển hướng giữa các trang HTML.
- **Tăng trải nghiệm người dùng**: Đảm bảo giao diện mượt mà và đồng bộ với logic nghiệp vụ.

### 4.2. Cấu Trúc
- **Class `UIManager`**:
  - Quản lý các chức năng UI như thông báo, modal, danh sách, và validation.
  - **Ví dụ**:
    ```javascript
    showSuccess(message, duration = 3000) {
        this.showNotification(message, 'success', duration);
    }
    ```
- **Object `Pages`**:
  - Chứa các đường dẫn đến các trang HTML:
    ```javascript
    export const Pages = {
        AUTH: 'auth.html',
        PROFILE: 'profile.html',
        // ...
    };
    ```

### 4.3. Cách Sử Dụng
- **Hiển thị thông báo**:
  - Sử dụng `showSuccess`, `showError`, `showWarning`, hoặc `showInfo`.
  - **Ví dụ**: `UI.showSuccess('Đăng nhập thành công!', 3000);`
- **Quản lý modal**:
  - Hiển thị: `UI.showModal('modalId');`
  - Ẩn: `UI.hideModal('modalId');`
- **Hiển thị danh sách**:
  - **Ví dụ**: Hiển thị kết quả tìm kiếm game:
    ```javascript
    UI.displayGameSearchResults(results, callback);
    ```
- **Thêm phương thức mới**:
  - Thêm vào `UIManager` để quản lý giao diện.
  - **Ví dụ**: Hiển thị danh sách bạn bè:
    ```javascript
    displayFriendsList(friends) {
        const container = document.getElementById('friendsList');
        if (!container) return;
        container.innerHTML = friends.length > 0
            ? friends.map(friend => `
                <div class="friend-item bg-gray-700 p-3 rounded">${friend.username}</div>
            `).join('')
            : '<div class="text-gray-400">No friends found</div>';
    }
    ```
- **Xóa phương thức**:
  - Xóa phương thức và kiểm tra xem có được gọi trong `handle.js` hoặc `event.js` không.

### 4.4. Lưu Ý
- Đảm bảo các ID phần tử HTML tồn tại trong DOM trước khi gọi phương thức UI.
- Sử dụng các class Tailwind CSS nhất quán để giữ giao diện đồng bộ.
- Kiểm tra các phương thức modal và thông báo để đảm bảo hoạt động đúng trên mọi trình duyệt.

---

## 5. Hướng Dẫn Chung Khi Chỉnh Sửa
- **Kiểm tra phụ thuộc**: Trước khi xóa hoặc sửa hàm, tìm kiếm xem hàm đó có được gọi ở đâu không (dùng Ctrl+F hoặc Cmd+F).
- **Kiểm tra lỗi**:
  - Thêm kiểm tra đầu vào và xử lý lỗi.
  - Sử dụng `console.log` hoặc `UI.showError` để debug.
- **Tối ưu hóa**:
  - Tránh gọi API không cần thiết.
  - Sử dụng `UI.showLoading` và `UI.hideLoading` để cải thiện trải nghiệm người dùng.
- **Kiểm tra trước khi đẩy code**:
  - Chạy thử trên môi trường local.
  - Kiểm tra các trường hợp biên (dữ liệu rỗng, lỗi mạng, v.v.).

---

## 6. Ví Dụ Tích Hợp Tính Năng Mới
Thêm tính năng "Lấy danh sách bạn bè":
1. **Trong `api.js`**:
   ```javascript
   async GetFriends(userId) {
       const query = API.buildQuery({ userId });
       const res = await this.GET(query);
       return res;
   }
   ```
2. **Trong `handle.js`**:
   ```javascript
   static async LoadFriends(userId) {
       const api = new APIUser();
       UI.showLoading();
       const res = await api.GetFriends(userId);
       UI.hideLoading();
       if (res.ok) {
           UI.displayFriendsList(res.data.friends);
           UI.showSuccess("Tải danh sách bạn bè thành công!");
       } else {
           UI.showWarning("Không thể tải danh sách bạn bè!");
       }
   }
   ```
3. **Trong `ui.js`**:
   ```javascript
   displayFriendsList(friends) {
       const container = document.getElementById('friendsList');
       if (!container) return;
       container.innerHTML = friends.length > 0
           ? friends.map(friend => `
               <div class="friend-item bg-gray-700 p-3 rounded">${friend.username}</div>
           `).join('')
           : '<div class="text-gray-400">No friends found</div>';
   }
   ```
4. **Trong `event.js`**:
   ```javascript
   listenIfExists('#view-friends', 'click', () => {
       Handle.LoadFriends('user123');
   });
   ```

---

## 7. Kết Luận
- **`event.js`**: Lắng nghe và xử lý các sự kiện người dùng, kết nối với `Handle` thông qua các hàm như `listenIfExists` và `listenWindow`.
- **`api.js`**: Đóng gói các thao tác giao tiếp server, cho phép gọi API dễ dàng với `await`.
- **`handle.js`**: Trung tâm điều phối, sử dụng `api.js` để lấy/gửi dữ liệu và `ui.js` để cập nhật giao diện.
- **`ui.js`**: Cung cấp các hàm tạo hiệu ứng giao diện nhanh gọn, đảm bảo trải nghiệm người dùng mượt mà.
- Khi chỉnh sửa hoặc thêm mới, tuân theo cấu trúc hiện có và kiểm tra kỹ lưỡng để tránh lỗi. Nếu cần hỗ trợ, liên hệ đội ngũ hoặc tham khảo tài liệu dự án.