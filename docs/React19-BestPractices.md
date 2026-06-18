# React 19 Best Practices & Documentation

> **Project**: Municipality Bus Ticketing System - Web Dashboard  
> **React Version**: 19 (Stable - December 2024)  
> **TypeScript**: 5.6+  
> **Last Updated**: 18.06.2026  

---

## 🚀 React 19 Yeni Özellikler

### 1. Actions (Form ve Data Mutations)

React 19, form submit ve data mutation işlemlerini basitleştiren yeni bir Actions API getiriyor.

#### useActionState Hook

Form submit durumlarını (pending, error, success) otomatik yönetir:

```typescript
import { useActionState } from 'react';

type FormState = {
  error: string | null;
  success: boolean;
};

const initialState: FormState = {
  error: null,
  success: false,
};

function LoginForm() {
  const [state, formAction, isPending] = useActionState(
    async (previousState: FormState, formData: FormData) => {
      const email = formData.get('email') as string;
      const password = formData.get('password') as string;

      // API call
      const result = await authenticate({ email, password });

      if (result.error) {
        return { error: result.error, success: false };
      }

      return { error: null, success: true };
    },
    initialState
  );

  return (
    <form action={formAction}>
      <input name="email" type="email" required />
      <input name="password" type="password" required />
      
      <button type="submit" disabled={isPending}>
        {isPending ? 'Loading...' : 'Login'}
      </button>

      {state.error && <p className="error">{state.error}</p>}
      {state.success && <p className="success">Login successful!</p>}
    </form>
  );
}
```

#### useFormStatus Hook

Form içindeki bileşenlerin submit durumuna erişim sağlar:

```typescript
import { useFormStatus } from 'react-dom';

function SubmitButton() {
  const { pending } = useFormStatus();

  return (
    <button type="submit" disabled={pending}>
      {pending ? 'Saving...' : 'Save'}
    </button>
  );
}

function EditProfile() {
  return (
    <form action={updateProfile}>
      <input name="name" defaultValue="John" />
      <SubmitButton />
    </form>
  );
}
```

#### useOptimistic Hook

Kullanıcıya anında feedback göstermek için optimistic updates:

```typescript
import { useOptimistic } from 'react';

function CommentList({ comments }: { comments: Comment[] }) {
  const [optimisticComments, setOptimisticComments] = useOptimistic(
    comments,
    (state, newComment: Comment) => [...state, newComment]
  );

  const addComment = async (formData: FormData) => {
    const text = formData.get('text') as string;
    const newComment = { id: Date.now(), text };

    // Optimistically update UI
    setOptimisticComments(newComment);

    // Then make the API call
    await submitComment(newComment);
  };

  return (
    <div>
      {optimisticComments.map((comment) => (
        <Comment key={comment.id} comment={comment} />
      ))}

      <form action={addComment}>
        <input name="text" required />
        <button type="submit">Add Comment</button>
      </form>
    </div>
  );
}
```

### 2. use() Hook - Async Resource Management

Promise'leri render içinde okumak için yeni API:

```typescript
import { use } from 'react';

function UserProfile({ userId }: { userId: Promise<string> }) {
  // Suspends until promise resolves
  const userIdSync = use(userId);

  const user = getUserById(userIdSync);

  return <div>Hello, {user.name}</div>;
}
```

#### Conditional Context Reading

```typescript
import { use } from 'react';

function Heading({ children }) {
  if (children == null) return null;

  // Works with early returns (unlike useContext)
  const theme = use(ThemeContext);

  return <h1 style={{ color: theme.color }}>{children}</h1>;
}
```

### 3. ref as a Prop

Function components artık ref prop olarak alabilir (forwardRef'a gerek yok):

```typescript
// Before React 19
const ForwardedInput = forwardRef<HTMLInputElement, InputProps>(
  ({ placeholder, ...props }, ref) => {
    return <input placeholder={placeholder} ref={ref} {...props} />;
  }
);

// React 19 - Simpler!
function Input({ placeholder, ref, ...props }: InputProps & { ref?: Ref<HTMLInputElement> }) {
  return <input placeholder={placeholder} ref={ref} {...props} />;
}
```

### 4. Context as Provider

```typescript
const ThemeContext = createContext('light');

function App({ children }) {
  return (
    <ThemeContext value="dark">
      {children}
    </ThemeContext>
  );
}
```

### 5. Document Metadata Support

```typescript
function BlogPost({ post }) {
  return (
    <article>
      <title>{post.title}</title>
      <meta name="author" content="Josh" />
      <link rel="author" href="https://twitter.com/joshcstory/" />
      
      <h1>{post.title}</h1>
      <p>{post.content}</p>
    </article>
  );
}
```

### 6. Stylesheet Management

```typescript
function ComponentWithStyles() {
  return (
    <Suspense fallback="loading...">
      <link rel="stylesheet" href="/styles.css" precedence="default" />
      <article>Content</article>
    </Suspense>
  );
}
```

### 7. Resource Preloading APIs

```typescript
import { prefetchDNS, preconnect, preload, preinit } from 'react-dom';

function MyApp() {
  preinit('https://api.example.com/script.js', { as: 'script' });
  preload('https://fonts.example.com/font.woff', { as: 'font' });
  prefetchDNS('https://api.example.com');
  preconnect('https://api.example.com');

  return <div>App</div>;
}
```

---

## 🎯 Best Practices 2025

### 1. Component Architecture

#### ✅ DO: Feature-Based Structure

```text
src/
├── features/
│   ├── auth/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── services/
│   │   └── types/
│   ├── dashboard/
│   ├── users/
│   └── tickets/
├── components/  # Shared UI components
├── hooks/       # Global custom hooks
└── lib/         # Utilities
```

#### ✅ DO: Component Composition

```typescript
// ✅ Good - Composable
function DataTable({ columns, data, onRowClick }) {
  return (
    <table>
      <thead>
        {columns.map(col => <th key={col.key}>{col.label}</th>)}
      </thead>
      <tbody>
        {data.map(row => (
          <tr key={row.id} onClick={() => onRowClick(row)}>
            {columns.map(col => <td>{row[col.key]}</td>)}
          </tr>
        ))}
      </tbody>
    </table>
  );
}

// ❌ Bad - Prop drilling
function BadComponent({ row1, row2, row3, col1, col2, col3 }) {
  // ...
}
```

### 2. State Management

#### ✅ DO: Server State vs Client State

```typescript
// Server State - TanStack Query
const { data: user, isLoading } = useQuery({
  queryKey: ['user', userId],
  queryFn: () => fetchUser(userId),
});

// Client State - Zustand
const useStore = create((set) => ({
  theme: 'light',
  setTheme: (theme) => set({ theme }),
}));

// Form State - React Hook Form
const { register, handleSubmit } = useForm();
```

#### ✅ DO: Optimistic Updates

```typescript
const { data: todos, mutate } = useQuery({ queryKey: ['todos'] });

const addTodo = useMutation({
  mutationFn: (todo: Todo) => api.createTodo(todo),
  onMutate: async (newTodo) => {
    // Cancel outgoing refetch
    await queryClient.cancelQuery({ queryKey: ['todos'] });

    // Optimistically update
    queryClient.setQueryData(['todos'], (old) => [...old, newTodo]);

    return { previousTodos: old };
  },
  onError: (err, newTodo, context) => {
    // Rollback
    queryClient.setQueryData(['todos'], context.previousTodos);
  },
});
```

### 3. TypeScript Integration

#### ✅ DO: Strict Type Safety

```typescript
// ✅ Good - Explicit types
interface User {
  id: string;
  name: string;
  email: string;
}

interface UserProfileProps {
  userId: string;
  onError?: (error: Error) => void;
}

function UserProfile({ userId, onError }: UserProfileProps) {
  // ...
}

// ❌ Bad - Any types
function BadComponent(props: any) {
  // ...
}
```

#### ✅ DO: Type-Safe Props

```typescript
// ✅ Good - React.FC yerine explicit props
function Button({ 
  label, 
  onClick, 
  variant = 'primary' 
}: ButtonProps) {
  return <button onClick={onClick}>{label}</button>;
}

// ❌ Bad - React.FC (props automatically inferred better)
const Button: React.FC<ButtonProps> = ({ label, onClick }) => {
  return <button onClick={onClick}>{label}</button>;
};
```

#### ✅ DO: Generic Components

```typescript
function List<T>({ items, renderItem }: { 
  items: T[]; 
  renderItem: (item: T) => React.ReactNode; 
}) {
  return (
    <ul>
      {items.map((item, index) => (
        <li key={index}>{renderItem(item)}</li>
      ))}
    </ul>
  );
}
```

### 4. Performance Optimization

#### ✅ DO: Code Splitting

```typescript
import { lazy, Suspense } from 'react';

const Dashboard = lazy(() => import('../features/dashboard'));
const Users = lazy(() => import('../features/users'));

function App() {
  return (
    <Suspense fallback={<LoadingSpinner />}>
      <Dashboard />
    </Suspense>
  );
}
```

#### ✅ DO: Memoization (React Compiler ile otomatik)

```typescript
// React 19'da React Compiler otomatik memoize eder
// Manuel useCallback/useMemo genellikle gerekmez

// ✅ Basit ve temiz
function Counter() {
  const [count, setCount] = useState(0);
  
  return (
    <button onClick={() => setCount(count + 1)}>
      Count: {count}
    </button>
  );
}
```

#### ✅ DO: Virtualization for Large Lists

```typescript
import { FixedSizeList } from 'react-window';

function LargeList({ items }) {
  const Row = ({ index, style }) => (
    <div style={style}>{items[index]}</div>
  );

  return (
    <FixedSizeList
      height={400}
      itemCount={items.length}
      itemSize={35}
      width="100%"
    >
      {Row}
    </FixedSizeList>
  );
}
```

### 5. Error Handling

#### ✅ DO: Error Boundaries

```typescript
import { ErrorBoundary } from 'react-error-boundary';

function App() {
  return (
    <ErrorBoundary
      fallback={<ErrorFallback />}
      onReset={() => console.log('Error recovered')}
    >
      <Dashboard />
    </ErrorBoundary>
  );
}

function ErrorFallback({ error, resetErrorBoundary }) {
  return (
    <div>
      <h2>Something went wrong</h2>
      <pre>{error.message}</pre>
      <button onClick={resetErrorBoundary}>Try again</button>
    </div>
  );
}
```

#### ✅ DO: Try-Catch in Async Operations

```typescript
async function fetchData() {
  try {
    const response = await api.getData();
    return response;
  } catch (error) {
    if (error instanceof TypeError) {
      // Network error
      showError('Network error');
    } else if (error instanceof ApiError) {
      // API error
      showError(error.message);
    } else {
      // Unknown error
      showError('An unexpected error occurred');
    }
    throw error;
  }
}
```

### 6. Form Handling

#### ✅ DO: React Hook Form + Zod

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

const loginSchema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

type LoginFormValues = z.infer<typeof loginSchema>;

function LoginForm() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormValues) => {
    await authenticate(data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <input
        {...register('email')}
        placeholder="Email"
      />
      {errors.email && <p>{errors.email.message}</p>}

      <input
        {...register('password')}
        type="password"
        placeholder="Password"
      />
      {errors.password && <p>{errors.password.message}</p>}

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Loading...' : 'Login'}
      </button>
    </form>
  );
}
```

### 7. Testing

#### ✅ DO: Component Testing with React Testing Library

```typescript
import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { Button } from './Button';

describe('Button', () => {
  it('renders with correct text', () => {
    render(<Button>Click me</Button>);
    expect(screen.getByRole('button', { name: /click me/i })).toBeInTheDocument();
  });

  it('calls onClick when clicked', () => {
    const handleClick = vi.fn();
    render(<Button onClick={handleClick}>Click</Button>);
    
    fireEvent.click(screen.getByRole('button'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });
});
```

#### ✅ DO: Integration Tests

```typescript
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { Login } from './Login';

describe('Login', () => {
  it('logs in successfully', async () => {
    const mockLogin = vi.fn().mockResolvedValue({ token: 'abc' });
    render(<Login onLogin={mockLogin} />);

    await userEvent.type(screen.getByLabelText(/email/i), 'test@example.com');
    await userEvent.type(screen.getByLabelText(/password/i), 'password123');
    await userEvent.click(screen.getByRole('button', { name: /login/i }));

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
      });
    });
  });
});
```

---

## 📦 Project Setup

### Vite Configuration

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@features': path.resolve(__dirname, './src/features'),
      '@hooks': path.resolve(__dirname, './src/hooks'),
      '@lib': path.resolve(__dirname, './src/lib'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
  build: {
    sourcemap: false,
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router'],
          'ui-vendor': ['shadcn/ui', 'tailwind-merge'],
          'chart-vendor': ['recharts'],
        },
      },
    },
  },
});
```

### TypeScript Configuration

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "useDefineForClassFields": true,
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"],
      "@components/*": ["src/components/*"],
      "@features/*": ["src/features/*"],
      "@hooks/*": ["src/hooks/*"],
      "@lib/*": ["src/lib/*"]
    }
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

---

## 🔗 Referanslar

- [React 19 Release Notes](https://react.dev/blog/2024/12/05/react-19)
- [React 19 Upgrade Guide](https://react.dev/blog/2024/04/25/react-19-upgrade-guide)
- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [TanStack Query](https://tanstack.com/query)
- [Zustand](https://github.com/pmndrs/zustand)
- [React Hook Form](https://react-hook-form.com/)
- [shadcn/ui](https://ui.shadcn.com/)

---

**Last Updated**: 18.06.2026  
**Author**: Özgür Can TURNA
