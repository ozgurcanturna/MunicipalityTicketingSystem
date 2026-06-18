# React 19 API Reference & Best Practices

> **Project**: Municipality Bus Ticketing System - Web Dashboard  
> **React Version**: 19 (Stable - December 2024)  
> **TypeScript**: 5.6+  
> **Last Updated**: 18.06.2026  

---

## 📚 React 19 Core Hooks

### useState

State variable declaration with initializer functions and updater functions.

```typescript
import { useState } from 'react';

// Basic usage
const [count, setCount] = useState(0);

// Initializer function (only runs once)
const [todos, setTodos] = useState(() => createTodos());

// Updater function (for multiple sequential updates)
setCount(c => c + 1); // ✅ Correct for multiple updates
setCount(count + 1); // ❌ Wrong for multiple updates in same handler
```

#### Key Features

- **Initializer Function**: Pass a function instead of a value to avoid expensive initialization on every render
- **Updater Function**: Use `setState(prev => newValue)` for multiple sequential state updates
- **State as Snapshot**: State variables capture values at the time of render

#### Best Practices for useState

```typescript
// ✅ DO: Use initializer function for expensive operations
const [data, setData] = useState(() => fetchInitialData());

// ✅ DO: Use updater function for sequential updates
const handleClick = () => {
  setCount(c => c + 1);
  setCount(c => c + 1);
  setCount(c => c + 1);
};

// ✅ DO: Spread for objects/arrays
setForm({ ...form, name: 'John' });
setTodos([...todos, newTodo]);

// ❌ DON'T: Mutate state directly
form.name = 'John';
setForm(form);
```

---

### useEffect

Synchronize components with external systems (network, DOM, timers, etc.).

```typescript
import { useEffect } from 'react';

// Basic usage
useEffect(() => {
  // Setup code
  const subscription = api.subscribe(props.id);
  
  // Cleanup code
  return () => {
    subscription.unsubscribe();
  };
}, [props.id]); // Dependencies array
```

#### Dependency Rules

1. **All reactive values must be dependencies**: Props, state, and variables declared in component body
2. **Empty array `[]`**: Runs only on mount and unmount
3. **No array**: Runs on every render
4. **Linter enforcement**: React linter will warn if dependencies are missing

#### Best Practices for useEffect

```typescript
// ✅ DO: Move dynamic objects/functions inside Effect
useEffect(() => {
  const options = {
    serverUrl: 'https://localhost:1234',
    roomId: roomId
  };
  const connection = createConnection(options);
  connection.connect();
  return () => connection.disconnect();
}, [roomId]); // ✅ Only roomId as dependency

// ✅ DO: Use updater functions to avoid dependencies
useEffect(() => {
  const interval = setInterval(() => {
    setCount(c => c + 1); // No need for count in dependencies
  }, 1000);
  return () => clearInterval(interval);
}, []); // ✅ Empty dependencies

// ✅ DO: Extract custom hooks for reusable logic
function useChatRoom({ roomId, serverUrl }) {
  useEffect(() => {
    const connection = createConnection(serverUrl, roomId);
    connection.connect();
    return () => connection.disconnect();
  }, [roomId, serverUrl]);
}

// ❌ DON'T: Create new objects on every render as dependencies
const options = { serverUrl, roomId }; // ❌ Different object every render
useEffect(() => {
  // ...
}, [options]); // ❌ Re-runs on every render
```

---

### useMemo

Cache calculation results between re-renders.

```typescript
import { useMemo } from 'react';

const visibleTodos = useMemo(() => {
  return filterTodos(todos, tab);
}, [todos, tab]); // Dependencies
```

#### When to Use

- Expensive calculations (filtering large arrays, complex computations)
- Creating stable objects/functions as dependencies for other hooks
- Passing memoized values to `memo` components

#### Best Practices (General)

```typescript
// ✅ DO: Use for expensive calculations
const expensiveData = useMemo(() => {
  return processData(largeDataset);
}, [largeDataset]);

// ✅ DO: Memoize dependencies for other hooks
const options = useMemo(() => ({
  matchMode: 'whole-word',
  text
}), [text]);

const visibleItems = useMemo(() => {
  return searchItems(allItems, options);
}, [allItems, options]);

// ✅ DO: Use with memo components
const MemoizedComponent = memo(function Component({ data }) {
  return <div>{data}</div>;
});

const memoizedData = useMemo(() => computeData(props), [props]);
return <MemoizedComponent data={memoizedData} />;

// ❌ DON'T: Use for simple calculations
const doubled = useMemo(() => numbers.map(n => n * 2), [numbers]);
// Just use: const doubled = numbers.map(n => n * 2);
```

---

### useCallback

Cache function definitions between re-renders.

```typescript
import { useCallback } from 'react';

const handleSubmit = useCallback((orderDetails) => {
  post('/product/' + productId + '/buy', {
    referrer,
    orderDetails
  });
}, [productId, referrer]);
```

#### When to Use useCallback

- Passing functions to `memo` components
- Functions used as dependencies in other hooks
- Custom hooks that return functions

#### Best Practices for useCallback

```typescript
// ✅ DO: Use with memo components
const handleSubmit = useCallback((data) => {
  onSubmit(data);
}, [onSubmit]);

return <MemoizedForm onSubmit={handleSubmit} />;

// ✅ DO: Use updater functions to reduce dependencies
const handleAddTodo = useCallback((text) => {
  const newTodo = { id: nextId++, text };
  setTodos(todos => [...todos, newTodo]); // No todos dependency needed
}, []);

// ✅ DO: Memoize custom hook returns
function useRouter() {
  const navigate = useCallback((url) => {
    dispatch({ type: 'navigate', url });
  }, [dispatch]);
  
  return { navigate };
}

// ❌ DON'T: Use unnecessarily
const handleClick = useCallback(() => {
  doSomething();
}, []);
// Just use: const handleClick = () => { doSomething(); };
```

---

### useRef

Reference values that persist without triggering re-renders.

```typescript
import { useRef } from 'react';

// DOM reference
const inputRef = useRef<HTMLInputElement>(null);

// Mutable value (not for rendering)
const intervalRef = useRef<number | null>(null);
const previousCountRef = useRef(0);
```

#### When to Use useRef

- Accessing DOM nodes directly
- Storing values that don't affect rendering
- Keeping track of values across renders without state

#### Best Practices for useRef

```typescript
// ✅ DO: Focus input
const inputRef = useRef<HTMLInputElement>(null);

function handleClick() {
  inputRef.current?.focus();
}

// ✅ DO: Store interval IDs
const intervalRef = useRef<number | null>(null);

useEffect(() => {
  intervalRef.current = setInterval(() => {
    // ...
  }, 1000);
  
  return () => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
    }
  };
}, []);

// ✅ DO: Access another component's DOM
function MyInput({ ref }) {
  return <input ref={ref} />;
}

function Form() {
  const inputRef = useRef(null);
  return <MyInput ref={inputRef} />;
}

// ❌ DON'T: Read/write during render
function Component() {
  myRef.current = 123; // ❌ Wrong
  return <div>{myOtherRef.current}</div>; // ❌ Wrong
}

// ✅ DO: Read/write in effects or event handlers
function Component() {
  useEffect(() => {
    myRef.current = 123; // ✅ Correct
  }, []);
  
  function handleClick() {
    myRef.current = 456; // ✅ Correct
  }
}
```

---

### useTransition

Defer non-urgent updates for better UX.

```typescript
import { useTransition } from 'react';

function Search() {
  const [isPending, startTransition] = useTransition();
  const [query, setQuery] = useState('');
  
  function handleChange(e) {
    startTransition(() => {
      setQuery(e.target.value); // Non-urgent update
    });
  }
  
  return (
    <>
      <input value={query} onChange={handleChange} />
      {isPending && <div>Loading...</div>}
      <Results query={query} />
    </>
  );
}
```

#### Best Practices for useTransition

```typescript
// ✅ DO: Use for search, filters, tabs
function TabSelector() {
  const [activeTab, setActiveTab] = useState('home');
  const [isPending, startTransition] = useTransition();
  
  function switchTab(tab: string) {
    startTransition(() => {
      setActiveTab(tab);
    });
  }
  
  return (
    <>
      <button onClick={() => switchTab('home')}>Home</button>
      <button onClick={() => switchTab('about')}>About</button>
      {isPending && <Spinner />}
      <TabContent tab={activeTab} />
    </>
  );
}

// ✅ DO: Combine with Suspense
function Search() {
  const [query, setQuery] = useState('');
  const [isPending, startTransition] = useTransition();
  
  function handleChange(e) {
    startTransition(() => {
      setQuery(e.target.value);
    });
  }
  
  return (
    <>
      <input value={query} onChange={handleChange} />
      <Suspense fallback={<Loading />}>
        <Results query={query} />
      </Suspense>
    </>
  );
}
```

---

### useOptimistic

Show instant UI feedback before server response.

```typescript
import { useOptimistic } from 'react';

function CommentList() {
  const [comments, setComments] = useState<Comment[]>([]);
  const [optimisticComments, setOptimisticComments] = useOptimistic(
    comments,
    (state, newComment: Comment) => [...state, newComment]
  );
  
  const addComment = async (formData: FormData) => {
    const newComment = { id: Date.now(), text: formData.get('text') };
    setOptimisticComments(newComment); // Show immediately
    await submitComment(newComment); // Then submit
  };
  
  return (
    <>
      {optimisticComments.map(c => <Comment key={c.id} comment={c} />)}
      <form action={addComment}>
        <input name="text" />
        <button type="submit">Add</button>
      </form>
    </>
  );
}
```

---

### useActionState

Handle form state with pending and error states.

```typescript
import { useActionState } from 'react';

const initialState = { error: null, success: false };

function LoginForm() {
  const [state, formAction, isPending] = useActionState(
    async (previousState, formData) => {
      const email = formData.get('email') as string;
      const password = formData.get('password') as string;
      
      const error = await authenticate({ email, password });
      if (error) return { error, success: false };
      return { error: null, success: true };
    },
    initialState
  );
  
  return (
    <form action={formAction}>
      <input name="email" type="email" />
      <input name="password" type="password" />
      <button type="submit" disabled={isPending}>
        {isPending ? 'Logging in...' : 'Login'}
      </button>
      {state.error && <p>Error: {state.error}</p>}
      {state.success && <p>Login successful!</p>}
    </form>
  );
}
```

---

### useFormStatus

Access form status from nested components.

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

function Form() {
  return (
    <form action={handleSubmit}>
      <input name="name" />
      <SubmitButton />
    </form>
  );
}
```

---

### use

Read resources (promises, context) in render.

```typescript
import { use } from 'react';

function UserProfile({ userIdPromise }: { userIdPromise: Promise<string> }) {
  const userId = use(userIdPromise); // Suspends until resolved
  const user = getUserById(userId);
  
  return <div>Hello, {user.name}</div>;
}

// Conditional context reading
function Heading({ children }) {
  if (children == null) return null;
  
  const theme = use(ThemeContext); // Works with early returns
  return <h1 style={{ color: theme.color }}>{children}</h1>;
}
```

---

### useLayoutEffect

Run after DOM mutations but before paint.

```typescript
import { useLayoutEffect, useRef } from 'react';

function Tooltip({ x, y }) {
  const tooltipRef = useRef<HTMLDivElement>(null);
  
  useLayoutEffect(() => {
    if (tooltipRef.current) {
      // Measure and position before paint
      const { width, height } = tooltipRef.current.getBoundingClientRect();
      tooltipRef.current.style.left = `${x - width / 2}px`;
      tooltipRef.current.style.top = `${y - height}px`;
    }
  }, [x, y]);
  
  return <div ref={tooltipRef}>Tooltip</div>;
}
```

---

### useEffectEvent (React 19)

Create non-reactive event handlers inside effects.

```typescript
import { useEffect, useEffectEvent } from 'react';

function Page({ url, shoppingCart }) {
  const onVisit = useEffectEvent((visitedUrl: string) => {
    logVisit(visitedUrl, shoppingCart.length); // Can read latest state
  });
  
  useEffect(() => {
    onVisit(url); // Only re-runs when url changes
  }, [url]); // shoppingCart not a dependency
}
```

---

## 🎯 React 19 New Features

### ref as a Prop

Function components can now receive `ref` as a prop.

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

### Context as Provider

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

### Document Metadata Support

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

### Stylesheet Management

```typescript
function ComponentWithStyles() {
  return (
    <Suspense fallback="loading...">
      <link rel="stylesheet" href="/styles.css" precedence="default" />
      <link rel="stylesheet" href="/high-priority.css" precedence="high" />
      <article>Content</article>
    </Suspense>
  );
}
```

### Resource Preloading APIs

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

## 📦 React DOM APIs

### createRoot

Client-side rendering with automatic batching.

```typescript
import { createRoot } from 'react-dom/client';

const root = createRoot(document.getElementById('root'));
root.render(<App />);
```

### hydrateRoot

Server-side rendering hydration.

```typescript
import { hydrateRoot } from 'react-dom/client';

const root = hydrateRoot(
  document.getElementById('root'),
  <App />,
  {
    onRecoverableError: (error, info) => {
      console.error('Hydration error:', error, info);
    }
  }
);
```

### flushSync

Force synchronous update.

```typescript
import { flushSync } from 'react-dom';

function handleClick() {
  flushSync(() => {
    setCount(count + 1);
  });
  // Screen updated immediately
}
```

---

## 🔧 React Compiler

Automatic memoization of components and values.

```typescript
// With React Compiler, you don't need manual useMemo/useCallback
function Counter() {
  const [count, setCount] = useState(0);
  
  const doubled = count * 2; // Automatically memoized
  const handleClick = () => setCount(count + 1); // Automatically memoized
  
  return <button onClick={handleClick}>Count: {doubled}</button>;
}
```

---

## 📚 References

- [React 19 Release Notes](https://react.dev/blog/2024/12/05/react-19)
- [React Documentation](https://react.dev)
- [React Compiler](https://react.dev/learn/react-compiler)
- [React Server Components](https://react.dev/reference/rsc/server-components)

---

**Last Updated**: 18.06.2026  
**Author**: Özgür Can TURNA
