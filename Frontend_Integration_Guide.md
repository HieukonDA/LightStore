# Frontend Integration Guide - SignalR Notifications

## 1. JavaScript Client Setup

### Installation
```bash
# NPM Installation
npm install @microsoft/signalr

# Or CDN (for vanilla JS)
<script src="https://unpkg.com/@microsoft/signalr@latest/dist/browser/signalr.min.js"></script>
```

### Basic Connection Setup
```javascript
// signalr-client.js
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class NotificationClient {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
    }

    async connect(token, userRole) {
        try {
            // Create connection
            this.connection = new HubConnectionBuilder()
                .withUrl('https://localhost:5264/notificationHub', {
                    accessTokenFactory: () => token,
                    transport: signalR.HttpTransportType.WebSockets,
                    skipNegotiation: true
                })
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.previousRetryCount < 3) {
                            return Math.random() * 10000;
                        } else {
                            return null; // Stop retrying
                        }
                    }
                })
                .configureLogging(LogLevel.Information)
                .build();

            // Setup event handlers
            this.setupEventHandlers();
            
            // Start connection
            await this.connection.start();
            console.log('✅ SignalR Connected');
            this.isConnected = true;
            this.reconnectAttempts = 0;

            // Join appropriate groups based on user role
            await this.joinGroups(userRole);

        } catch (error) {
            console.error('❌ SignalR Connection failed:', error);
            this.handleConnectionError(error);
        }
    }

    async joinGroups(userRole) {
        if (!this.connection || this.connection.state !== 'Connected') {
            console.warn('Connection not ready for group joining');
            return;
        }

        try {
            if (userRole === 'Admin' || userRole === 'Staff') {
                await this.connection.invoke('JoinAdminGroup');
                console.log('✅ Joined Admin Group');
            } else if (userRole === 'Customer') {
                await this.connection.invoke('JoinCustomerGroup');
                console.log('✅ Joined Customer Group');
            }
        } catch (error) {
            console.error('❌ Failed to join groups:', error);
        }
    }

    setupEventHandlers() {
        // Receive notifications
        this.connection.on('ReceiveNotification', (notification) => {
            this.handleNotification(notification);
        });

        // Connection events
        this.connection.onreconnecting(() => {
            console.log('🔄 SignalR Reconnecting...');
            this.isConnected = false;
        });

        this.connection.onreconnected(() => {
            console.log('✅ SignalR Reconnected');
            this.isConnected = true;
        });

        this.connection.onclose(() => {
            console.log('❌ SignalR Disconnected');
            this.isConnected = false;
            this.attemptReconnect();
        });
    }

    handleNotification(notification) {
        console.log('📱 New Notification:', notification);
        
        // Display notification in UI
        this.showNotification(notification);
        
        // Update notification counter
        this.updateNotificationCounter();
        
        // Play notification sound (optional)
        this.playNotificationSound();
        
        // Store in local storage for offline access
        this.storeNotificationLocally(notification);
    }

    showNotification(notification) {
        // Browser notification
        if (Notification.permission === 'granted') {
            new Notification(notification.title, {
                body: notification.content,
                icon: '/favicon.ico',
                tag: notification.id
            });
        }

        // Toast notification in app
        this.showToast(notification);
        
        // Add to notification list
        this.addToNotificationList(notification);
    }

    async disconnect() {
        if (this.connection) {
            try {
                await this.connection.stop();
                console.log('✅ SignalR Disconnected gracefully');
            } catch (error) {
                console.error('❌ Error during disconnect:', error);
            }
        }
    }

    // Reconnection logic
    async attemptReconnect() {
        if (this.reconnectAttempts >= this.maxReconnectAttempts) {
            console.log('❌ Max reconnection attempts reached');
            return;
        }

        this.reconnectAttempts++;
        const delay = Math.min(1000 * Math.pow(2, this.reconnectAttempts), 30000);
        
        console.log(`🔄 Attempting reconnection #${this.reconnectAttempts} in ${delay}ms`);
        
        setTimeout(async () => {
            try {
                await this.connection.start();
            } catch (error) {
                console.error('❌ Reconnection failed:', error);
                this.attemptReconnect();
            }
        }, delay);
    }
}

// Export singleton instance
export const notificationClient = new NotificationClient();
```

## 2. React Implementation

### React Hook for Notifications
```javascript
// hooks/useNotifications.js
import { useState, useEffect, useCallback } from 'react';
import { notificationClient } from '../services/signalr-client';
import { useAuth } from './useAuth';

export const useNotifications = () => {
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [isConnected, setIsConnected] = useState(false);
    const { user, token } = useAuth();

    // Initialize SignalR connection
    useEffect(() => {
        if (user && token) {
            initializeConnection();
        }

        return () => {
            notificationClient.disconnect();
        };
    }, [user, token]);

    const initializeConnection = async () => {
        try {
            await notificationClient.connect(token, user.role);
            setIsConnected(true);
            
            // Setup notification handler
            notificationClient.connection.on('ReceiveNotification', handleNewNotification);
            
            // Load existing notifications
            await loadNotifications();
        } catch (error) {
            console.error('Failed to initialize notifications:', error);
        }
    };

    const handleNewNotification = useCallback((notification) => {
        setNotifications(prev => [notification, ...prev]);
        setUnreadCount(prev => prev + 1);
        
        // Show browser notification if permission granted
        if (Notification.permission === 'granted') {
            new Notification(notification.title, {
                body: notification.content,
                icon: '/favicon.ico'
            });
        }
    }, []);

    const loadNotifications = async () => {
        try {
            const endpoint = user.role === 'Customer' 
                ? '/api/customer-notifications'
                : '/api/notifications';
                
            const response = await fetch(endpoint, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                const data = await response.json();
                setNotifications(data.data);
                setUnreadCount(data.data.filter(n => !n.isRead).length);
            }
        } catch (error) {
            console.error('Failed to load notifications:', error);
        }
    };

    const markAsRead = async (notificationId) => {
        try {
            const endpoint = user.role === 'Customer'
                ? `/api/customer-notifications/${notificationId}/read`
                : `/api/notifications/${notificationId}/read`;

            const response = await fetch(endpoint, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                setNotifications(prev => prev.map(n => 
                    n.id === notificationId ? { ...n, isRead: true } : n
                ));
                setUnreadCount(prev => Math.max(0, prev - 1));
            }
        } catch (error) {
            console.error('Failed to mark notification as read:', error);
        }
    };

    const markAllAsRead = async () => {
        try {
            const endpoint = user.role === 'Customer'
                ? '/api/customer-notifications/read-all'
                : '/api/notifications/read-all';

            const response = await fetch(endpoint, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                setNotifications(prev => prev.map(n => ({ ...n, isRead: true })));
                setUnreadCount(0);
            }
        } catch (error) {
            console.error('Failed to mark all as read:', error);
        }
    };

    const deleteNotification = async (notificationId) => {
        try {
            const endpoint = user.role === 'Customer'
                ? `/api/customer-notifications/${notificationId}`
                : `/api/notifications/${notificationId}`;

            const response = await fetch(endpoint, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                setNotifications(prev => prev.filter(n => n.id !== notificationId));
                setUnreadCount(prev => {
                    const notification = notifications.find(n => n.id === notificationId);
                    return notification && !notification.isRead ? prev - 1 : prev;
                });
            }
        } catch (error) {
            console.error('Failed to delete notification:', error);
        }
    };

    return {
        notifications,
        unreadCount,
        isConnected,
        markAsRead,
        markAllAsRead,
        deleteNotification,
        refresh: loadNotifications
    };
};
```

### Notification Components
```javascript
// components/NotificationBell.jsx
import React from 'react';
import { Bell, BellRing } from 'lucide-react';
import { useNotifications } from '../hooks/useNotifications';

export const NotificationBell = ({ onClick }) => {
    const { unreadCount, isConnected } = useNotifications();

    return (
        <button
            onClick={onClick}
            className="relative p-2 text-gray-600 hover:text-gray-900 transition-colors"
        >
            {unreadCount > 0 ? (
                <BellRing className="w-6 h-6 text-blue-600" />
            ) : (
                <Bell className="w-6 h-6" />
            )}
            
            {/* Connection status indicator */}
            <div className={`absolute top-0 right-0 w-2 h-2 rounded-full ${
                isConnected ? 'bg-green-500' : 'bg-red-500'
            }`} />
            
            {/* Unread count badge */}
            {unreadCount > 0 && (
                <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                    {unreadCount > 99 ? '99+' : unreadCount}
                </span>
            )}
        </button>
    );
};
```

```javascript
// components/NotificationDropdown.jsx
import React, { useState } from 'react';
import { formatDistanceToNow } from 'date-fns';
import { X, Check, CheckCheck, Trash2 } from 'lucide-react';
import { useNotifications } from '../hooks/useNotifications';

export const NotificationDropdown = ({ isOpen, onClose }) => {
    const { 
        notifications, 
        unreadCount, 
        markAsRead, 
        markAllAsRead, 
        deleteNotification 
    } = useNotifications();

    if (!isOpen) return null;

    return (
        <div className="absolute right-0 top-full mt-2 w-96 bg-white rounded-lg shadow-xl border z-50">
            {/* Header */}
            <div className="flex items-center justify-between p-4 border-b">
                <h3 className="text-lg font-semibold">Thông báo</h3>
                <div className="flex items-center gap-2">
                    {unreadCount > 0 && (
                        <button
                            onClick={markAllAsRead}
                            className="text-sm text-blue-600 hover:text-blue-800"
                        >
                            <CheckCheck className="w-4 h-4" />
                        </button>
                    )}
                    <button onClick={onClose}>
                        <X className="w-5 h-5 text-gray-500" />
                    </button>
                </div>
            </div>

            {/* Notifications list */}
            <div className="max-h-96 overflow-y-auto">
                {notifications.length === 0 ? (
                    <div className="p-8 text-center text-gray-500">
                        Không có thông báo nào
                    </div>
                ) : (
                    notifications.map((notification) => (
                        <NotificationItem
                            key={notification.id}
                            notification={notification}
                            onMarkAsRead={markAsRead}
                            onDelete={deleteNotification}
                        />
                    ))
                )}
            </div>

            {/* Footer */}
            {notifications.length > 0 && (
                <div className="p-3 border-t text-center">
                    <a href="/notifications" className="text-sm text-blue-600 hover:text-blue-800">
                        Xem tất cả thông báo
                    </a>
                </div>
            )}
        </div>
    );
};

const NotificationItem = ({ notification, onMarkAsRead, onDelete }) => {
    const [isHovered, setIsHovered] = useState(false);

    const handleMarkAsRead = (e) => {
        e.stopPropagation();
        onMarkAsRead(notification.id);
    };

    const handleDelete = (e) => {
        e.stopPropagation();
        onDelete(notification.id);
    };

    return (
        <div
            className={`p-4 border-b hover:bg-gray-50 transition-colors ${
                !notification.isRead ? 'bg-blue-50' : ''
            }`}
            onMouseEnter={() => setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
        >
            <div className="flex items-start justify-between">
                <div className="flex-1">
                    <div className="flex items-center gap-2">
                        <h4 className="font-medium text-gray-900">
                            {notification.title}
                        </h4>
                        {!notification.isRead && (
                            <div className="w-2 h-2 bg-blue-500 rounded-full" />
                        )}
                    </div>
                    <p className="text-sm text-gray-600 mt-1">
                        {notification.content}
                    </p>
                    <p className="text-xs text-gray-400 mt-2">
                        {formatDistanceToNow(new Date(notification.createdAt), { 
                            addSuffix: true 
                        })}
                    </p>
                </div>
                
                {/* Action buttons */}
                {isHovered && (
                    <div className="flex items-center gap-1 ml-2">
                        {!notification.isRead && (
                            <button
                                onClick={handleMarkAsRead}
                                className="p-1 text-gray-400 hover:text-blue-600"
                                title="Đánh dấu đã đọc"
                            >
                                <Check className="w-4 h-4" />
                            </button>
                        )}
                        <button
                            onClick={handleDelete}
                            className="p-1 text-gray-400 hover:text-red-600"
                            title="Xóa thông báo"
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
};
```

## 3. Vue.js Implementation

### Vue Composition API
```javascript
// composables/useNotifications.js
import { ref, reactive, onMounted, onUnmounted } from 'vue';
import { notificationClient } from '../services/signalr-client';
import { useAuthStore } from '../stores/auth';

export function useNotifications() {
    const authStore = useAuthStore();
    
    const notifications = ref([]);
    const unreadCount = ref(0);
    const isConnected = ref(false);
    
    let connection = null;

    const initializeConnection = async () => {
        if (!authStore.user || !authStore.token) return;

        try {
            await notificationClient.connect(authStore.token, authStore.user.role);
            isConnected.value = true;
            
            // Setup notification handler
            notificationClient.connection.on('ReceiveNotification', handleNewNotification);
            
            // Load existing notifications
            await loadNotifications();
        } catch (error) {
            console.error('Failed to initialize notifications:', error);
        }
    };

    const handleNewNotification = (notification) => {
        notifications.value.unshift(notification);
        unreadCount.value++;
        
        // Show browser notification
        if (Notification.permission === 'granted') {
            new Notification(notification.title, {
                body: notification.content,
                icon: '/favicon.ico'
            });
        }
    };

    const loadNotifications = async () => {
        try {
            const endpoint = authStore.user.role === 'Customer' 
                ? '/api/customer-notifications'
                : '/api/notifications';
                
            const response = await fetch(endpoint, {
                headers: {
                    'Authorization': `Bearer ${authStore.token}`,
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                const data = await response.json();
                notifications.value = data.data;
                unreadCount.value = data.data.filter(n => !n.isRead).length;
            }
        } catch (error) {
            console.error('Failed to load notifications:', error);
        }
    };

    const markAsRead = async (notificationId) => {
        try {
            const endpoint = authStore.user.role === 'Customer'
                ? `/api/customer-notifications/${notificationId}/read`
                : `/api/notifications/${notificationId}/read`;

            const response = await fetch(endpoint, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${authStore.token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const notification = notifications.value.find(n => n.id === notificationId);
                if (notification && !notification.isRead) {
                    notification.isRead = true;
                    unreadCount.value--;
                }
            }
        } catch (error) {
            console.error('Failed to mark notification as read:', error);
        }
    };

    onMounted(() => {
        initializeConnection();
    });

    onUnmounted(() => {
        if (connection) {
            notificationClient.disconnect();
        }
    });

    return {
        notifications,
        unreadCount,
        isConnected,
        markAsRead,
        loadNotifications
    };
}
```

### Vue Component
```vue
<!-- components/NotificationBell.vue -->
<template>
  <div class="relative">
    <button
      @click="toggleDropdown"
      class="relative p-2 text-gray-600 hover:text-gray-900 transition-colors"
    >
      <BellIcon 
        :class="[
          'w-6 h-6',
          unreadCount > 0 ? 'text-blue-600' : 'text-gray-600'
        ]"
      />
      
      <!-- Connection status -->
      <div 
        :class="[
          'absolute top-0 right-0 w-2 h-2 rounded-full',
          isConnected ? 'bg-green-500' : 'bg-red-500'
        ]"
      />
      
      <!-- Unread badge -->
      <span
        v-if="unreadCount > 0"
        class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center"
      >
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
    </button>

    <!-- Dropdown -->
    <NotificationDropdown
      v-if="showDropdown"
      :notifications="notifications"
      :unread-count="unreadCount"
      @close="showDropdown = false"
      @mark-as-read="markAsRead"
      @mark-all-read="markAllAsRead"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { BellIcon } from '@heroicons/vue/24/outline';
import { useNotifications } from '../composables/useNotifications';
import NotificationDropdown from './NotificationDropdown.vue';

const showDropdown = ref(false);
const { notifications, unreadCount, isConnected, markAsRead, markAllAsRead } = useNotifications();

const toggleDropdown = () => {
  showDropdown.value = !showDropdown.value;
};
</script>
```

## 4. Angular Implementation

### Angular Service
```typescript
// services/notification.service.ts
import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface Notification {
  id: string;
  title: string;
  content: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService implements OnDestroy {
  private connection: HubConnection | null = null;
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  private unreadCountSubject = new BehaviorSubject<number>(0);
  private isConnectedSubject = new BehaviorSubject<boolean>(false);

  public notifications$ = this.notificationsSubject.asObservable();
  public unreadCount$ = this.unreadCountSubject.asObservable();
  public isConnected$ = this.isConnectedSubject.asObservable();

  constructor(private authService: AuthService) {
    this.initializeConnection();
  }

  private async initializeConnection() {
    const user = await this.authService.getCurrentUser();
    const token = this.authService.getToken();

    if (!user || !token) return;

    this.connection = new HubConnectionBuilder()
      .withUrl('https://localhost:5264/notificationHub', {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets,
        skipNegotiation: true
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.setupEventHandlers();

    try {
      await this.connection.start();
      this.isConnectedSubject.next(true);
      await this.joinGroups(user.role);
      await this.loadNotifications();
    } catch (error) {
      console.error('SignalR connection failed:', error);
    }
  }

  private setupEventHandlers() {
    if (!this.connection) return;

    this.connection.on('ReceiveNotification', (notification: Notification) => {
      const current = this.notificationsSubject.value;
      this.notificationsSubject.next([notification, ...current]);
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
      this.showBrowserNotification(notification);
    });

    this.connection.onreconnected(() => {
      this.isConnectedSubject.next(true);
    });

    this.connection.onclose(() => {
      this.isConnectedSubject.next(false);
    });
  }

  private async joinGroups(userRole: string) {
    if (!this.connection || this.connection.state !== 'Connected') return;

    try {
      if (userRole === 'Admin' || userRole === 'Staff') {
        await this.connection.invoke('JoinAdminGroup');
      } else if (userRole === 'Customer') {
        await this.connection.invoke('JoinCustomerGroup');
      }
    } catch (error) {
      console.error('Failed to join groups:', error);
    }
  }

  async markAsRead(notificationId: string): Promise<void> {
    // Implementation similar to React example
  }

  async markAllAsRead(): Promise<void> {
    // Implementation similar to React example
  }

  private showBrowserNotification(notification: Notification) {
    if (Notification.permission === 'granted') {
      new Notification(notification.title, {
        body: notification.content,
        icon: '/favicon.ico'
      });
    }
  }

  ngOnDestroy() {
    if (this.connection) {
      this.connection.stop();
    }
  }
}
```

### Angular Component
```typescript
// components/notification-bell.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from 'rxjs';
import { NotificationService, Notification } from '../services/notification.service';

@Component({
  selector: 'app-notification-bell',
  template: `
    <div class="relative">
      <button 
        (click)="toggleDropdown()"
        class="relative p-2 text-gray-600 hover:text-gray-900 transition-colors"
      >
        <svg 
          class="w-6 h-6"
          [class.text-blue-600]="(unreadCount$ | async)! > 0"
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
        </svg>
        
        <!-- Connection status -->
        <div 
          class="absolute top-0 right-0 w-2 h-2 rounded-full"
          [class.bg-green-500]="isConnected$ | async"
          [class.bg-red-500]="!(isConnected$ | async)"
        ></div>
        
        <!-- Unread badge -->
        <span 
          *ngIf="(unreadCount$ | async)! > 0"
          class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center"
        >
          {{ (unreadCount$ | async)! > 99 ? '99+' : (unreadCount$ | async) }}
        </span>
      </button>

      <!-- Dropdown -->
      <app-notification-dropdown
        *ngIf="showDropdown"
        [notifications]="notifications$ | async"
        [unreadCount]="unreadCount$ | async"
        (close)="showDropdown = false"
        (markAsRead)="markAsRead($event)"
        (markAllRead)="markAllAsRead()"
      ></app-notification-dropdown>
    </div>
  `,
  styleUrls: ['./notification-bell.component.scss']
})
export class NotificationBellComponent implements OnInit {
  notifications$: Observable<Notification[]>;
  unreadCount$: Observable<number>;
  isConnected$: Observable<boolean>;
  showDropdown = false;

  constructor(private notificationService: NotificationService) {
    this.notifications$ = this.notificationService.notifications$;
    this.unreadCount$ = this.notificationService.unreadCount$;
    this.isConnected$ = this.notificationService.isConnected$;
  }

  ngOnInit() {}

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
  }

  async markAsRead(notificationId: string) {
    await this.notificationService.markAsRead(notificationId);
  }

  async markAllAsRead() {
    await this.notificationService.markAllAsRead();
  }
}
```

## 5. Error Handling & Best Practices

### Connection Error Handling
```javascript
// error-handler.js
export class SignalRErrorHandler {
    static handleConnectionError(error) {
        if (error.name === 'HttpError') {
            if (error.statusCode === 401) {
                // Token expired - redirect to login
                window.location.href = '/login';
                return;
            }
        }
        
        if (error.name === 'TimeoutError') {
            // Connection timeout - show retry option
            this.showRetryDialog();
            return;
        }
        
        // Generic error handling
        console.error('SignalR Error:', error);
        this.showErrorMessage('Kết nối thất bại. Vui lòng thử lại.');
    }

    static showRetryDialog() {
        // Show UI dialog for retry
        if (confirm('Kết nối bị gián đoạn. Bạn có muốn thử lại không?')) {
            // Retry connection
            notificationClient.connect();
        }
    }

    static showErrorMessage(message) {
        // Show toast or alert
        console.error(message);
    }
}
```

### Performance Optimization
```javascript
// performance-optimizations.js

// Throttle notification updates
import { throttle } from 'lodash';

const throttledNotificationUpdate = throttle((notifications) => {
    // Update UI with new notifications
    updateNotificationUI(notifications);
}, 1000); // Max 1 update per second

// Batch notification operations
class NotificationBatcher {
    constructor() {
        this.batch = [];
        this.batchTimeout = null;
    }

    addNotification(notification) {
        this.batch.push(notification);
        
        if (this.batchTimeout) {
            clearTimeout(this.batchTimeout);
        }
        
        this.batchTimeout = setTimeout(() => {
            this.processBatch();
        }, 500); // Process batch after 500ms
    }

    processBatch() {
        if (this.batch.length > 0) {
            // Process all notifications in batch
            this.updateUI(this.batch);
            this.batch = [];
        }
    }
}

// Memory management
const MAX_NOTIFICATIONS = 100;

function manageNotificationMemory(notifications) {
    if (notifications.length > MAX_NOTIFICATIONS) {
        // Keep only the latest notifications
        return notifications.slice(0, MAX_NOTIFICATIONS);
    }
    return notifications;
}
```

### Browser Notification Permission
```javascript
// notification-permission.js
export class NotificationPermissionManager {
    static async requestPermission() {
        if (!('Notification' in window)) {
            console.warn('This browser does not support notifications');
            return false;
        }

        if (Notification.permission === 'granted') {
            return true;
        }

        if (Notification.permission === 'denied') {
            return false;
        }

        // Request permission
        const permission = await Notification.requestPermission();
        return permission === 'granted';
    }

    static async setupNotifications() {
        const hasPermission = await this.requestPermission();
        
        if (hasPermission) {
            console.log('✅ Browser notifications enabled');
        } else {
            console.log('❌ Browser notifications denied');
            // Show alternative notification method (toast, etc.)
        }
        
        return hasPermission;
    }

    static showBrowserNotification(title, options = {}) {
        if (Notification.permission === 'granted') {
            return new Notification(title, {
                icon: '/favicon.ico',
                badge: '/badge-icon.png',
                tag: 'lightstore-notification',
                renotify: true,
                ...options
            });
        }
        return null;
    }
}
```

## 6. Testing Frontend Integration

### Jest Unit Tests
```javascript
// __tests__/notification.test.js
import { render, screen, waitFor } from '@testing-library/react';
import { notificationClient } from '../services/signalr-client';
import { NotificationBell } from '../components/NotificationBell';

// Mock SignalR
jest.mock('@microsoft/signalr', () => ({
    HubConnectionBuilder: jest.fn().mockImplementation(() => ({
        withUrl: jest.fn().mockReturnThis(),
        withAutomaticReconnect: jest.fn().mockReturnThis(),
        configureLogging: jest.fn().mockReturnThis(),
        build: jest.fn(() => ({
            start: jest.fn(),
            stop: jest.fn(),
            on: jest.fn(),
            invoke: jest.fn(),
            state: 'Connected'
        }))
    })),
    LogLevel: { Information: 1 }
}));

describe('NotificationBell', () => {
    test('displays unread count correctly', async () => {
        // Mock useNotifications hook
        const mockNotifications = {
            unreadCount: 5,
            isConnected: true,
            notifications: []
        };
        
        render(<NotificationBell />);
        
        await waitFor(() => {
            expect(screen.getByText('5')).toBeInTheDocument();
        });
    });

    test('shows connection status indicator', () => {
        render(<NotificationBell />);
        
        const indicator = screen.getByRole('button').querySelector('.w-2');
        expect(indicator).toHaveClass('bg-green-500');
    });
});
```

### E2E Testing with Cypress
```javascript
// cypress/integration/notifications.spec.js
describe('Notification System', () => {
    beforeEach(() => {
        // Login as admin
        cy.login('admin@test.com', 'password');
        cy.visit('/dashboard');
    });

    it('should receive real-time notifications', () => {
        // Wait for SignalR connection
        cy.wait(2000);
        
        // Trigger a notification (e.g., create new order)
        cy.request('POST', '/api/orders', { /* order data */ });
        
        // Verify notification appears
        cy.get('[data-testid="notification-bell"]')
          .should('contain', '1');
          
        // Open notification dropdown
        cy.get('[data-testid="notification-bell"]').click();
        
        // Verify notification content
        cy.get('[data-testid="notification-dropdown"]')
          .should('be.visible')
          .and('contain', 'Đơn hàng mới');
    });

    it('should mark notifications as read', () => {
        // Open notifications
        cy.get('[data-testid="notification-bell"]').click();
        
        // Click on first notification
        cy.get('[data-testid="notification-item"]').first().click();
        
        // Verify unread count decreased
        cy.get('[data-testid="notification-bell"]')
          .should('not.contain', '1');
    });
});
```

## 7. Deployment Considerations

### Production Build Configuration
```javascript
// webpack.config.js (for React)
module.exports = {
    // ... other config
    resolve: {
        fallback: {
            "buffer": require.resolve("buffer"),
            "events": require.resolve("events"),
        }
    },
    optimization: {
        splitChunks: {
            cacheGroups: {
                signalr: {
                    test: /[\\/]node_modules[\\/]@microsoft[\\/]signalr/,
                    name: 'signalr',
                    chunks: 'all',
                }
            }
        }
    }
};
```

### Environment Variables
```bash
# .env.production
REACT_APP_API_URL=https://api.lightstore.com
REACT_APP_HUB_URL=https://api.lightstore.com/notificationHub
REACT_APP_ENVIRONMENT=production
```

```javascript
// config.js
export const config = {
    apiUrl: process.env.REACT_APP_API_URL || 'http://localhost:5264',
    hubUrl: process.env.REACT_APP_HUB_URL || 'http://localhost:5264/notificationHub',
    environment: process.env.REACT_APP_ENVIRONMENT || 'development'
};
```

### CDN and Caching
```html
<!-- For CDN delivery -->
<script src="https://unpkg.com/@microsoft/signalr@latest/dist/browser/signalr.min.js"></script>

<!-- Service Worker for offline notifications -->
<script>
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('/sw.js');
}
</script>
```